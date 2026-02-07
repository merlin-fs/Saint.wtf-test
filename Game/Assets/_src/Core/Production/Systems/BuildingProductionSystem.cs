using System;
using System.Collections.Generic;
using System.Linq;
using FTg.Common.Observables;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Transfers;

namespace Game.Core.Production
{
    public sealed class BuildingProductionSystem : ITickSystem, IDisposable
    {
        private readonly IList<BuildingModel> _buildings;
        private readonly ITransferScheduler _scheduler;
        // to route Finished events to specific building & action
        private readonly Dictionary<int, TransferRoute> _routes = new();
        private readonly IDisposable _subFinished;

        public BuildingProductionSystem(
            IList<BuildingModel> buildings,
            ITransferScheduler scheduler,
            ITransferStream transferStream)
        {
            _buildings = buildings ?? throw new ArgumentNullException(nameof(buildings));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            if (transferStream == null) throw new ArgumentNullException(nameof(transferStream));

            // subscribe to finished to update building pending counters
            _subFinished = transferStream.Finished.Subscribe(OnTransferFinished);
        }

        public void Dispose() => _subFinished.Dispose();

        public void Tick(float dt)
        {
            foreach (var t in _buildings)
            {
                TickBuilding(t, dt);
            }
        }

        private void TickBuilding(BuildingModel b, float dt)
        {
            // allow auto-restart when conditions become ok again
            if (b.Status == BuildingStatus.Stopped)
            {
                if (CanStartCycle(b, out var reason))
                {
                    b.StopReason = StopReason.None;
                    b.ProductionProgress = 0f;
                    b.TransferFailed = false;

                    // start next cycle
                    EnterIdle(b);
                }
                else
                {
                    b.StopReason = reason;
                    return;
                }
            }

            switch (b.Status)
            {
                case BuildingStatus.Idle:
                {
                    if (!CanStartCycle(b, out var reason))
                    {
                        Stop(b, reason);
                        return;
                    }

                    if (b.Recipe.HasInputs())
                        EnterPullInputs(b);
                    else
                        EnterProducing(b);

                    break;
                }

                case BuildingStatus.PullInputs:
                {
                    // schedule pulls once
                    if (!b.PullScheduled)
                    {
                        if (!SchedulePullInputs(b))
                        {
                            // Could not enqueue required pulls (likely NoInput / blocked)
                            Stop(b, b.StopReason == StopReason.None ? StopReason.NoInput : b.StopReason);
                            return;
                        }
                        b.PullScheduled = true;
                    }

                    // wait until all pull transfers finish
                    if (b.PendingPullTransfers <= 0)
                    {
                        // consume inputs from InPort instantly (internal)
                        if (!ConsumeInputsFromInPort(b))
                        {
                            // should not happen if pulls succeeded; treat as blocked
                            Stop(b, StopReason.TransferBlocked);
                            return;
                        }

                        EnterProducing(b);
                    }
                    break;
                }

                case BuildingStatus.Producing:
                {
                    var time = b.Recipe.ProductionTimeSeconds <= 0f ? 0.0001f : b.Recipe.ProductionTimeSeconds;

                    b.ProduceTimer += dt;
                    var p = b.ProduceTimer / time;
                    if (p > 1f) p = 1f;

                    b.ProductionProgress = p;

                    if (p >= 1f)
                    {
                        // spawn output into OutPort instantly (internal)
                        // If OutPort has no space -> treat as output blocked (usually OutPort capacity=1)
                        if (!b.OutPort.TryAddInstant(b.Recipe.Output))
                        {
                            Stop(b, StopReason.OutputFull);
                            return;
                        }

                        EnterPushOutput(b);
                    }
                    break;
                }

                case BuildingStatus.PushOutput:
                {
                    // schedule push once
                    if (!b.PushScheduled)
                    {
                        if (!SchedulePushOutput(b))
                        {
                            // most common: OutputStorage full
                            Stop(b, b.StopReason == StopReason.None ? StopReason.OutputFull : b.StopReason);
                            return;
                        }
                        b.PushScheduled = true;
                    }

                    // wait until push transfer completes
                    if (b.PendingPushTransfers <= 0)
                    {
                        EnterIdle(b);
                    }
                    break;
                }
                case BuildingStatus.Stopped:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // ---------------- state transitions ----------------

        private static void EnterIdle(BuildingModel b)
        {
            b.Status = BuildingStatus.Idle;
            b.ProductionProgress = 0f;

            b.PullScheduled = false;
            b.PushScheduled = false;
            b.PendingPullTransfers = 0;
            b.PendingPushTransfers = 0;
            b.TransferFailed = false;
        }

        private static void EnterPullInputs(BuildingModel b)
        {
            b.Status = BuildingStatus.PullInputs;
            b.ProductionProgress = 0f;

            b.PullScheduled = false;
            b.PendingPullTransfers = 0;
            b.TransferFailed = false;
        }

        private static void EnterProducing(BuildingModel b)
        {
            b.Status = BuildingStatus.Producing;
            b.ProduceTimer = 0f;
            b.ProductionProgress = 0f;
        }

        private static void EnterPushOutput(BuildingModel b)
        {
            b.Status = BuildingStatus.PushOutput;
            b.PushScheduled = false;
            b.PendingPushTransfers = 0;
        }

        private static void Stop(BuildingModel b, StopReason reason)
        {
            b.Status = BuildingStatus.Stopped;
            b.StopReason = reason;

            // keep progress as-is (can be useful for UI), or reset if you want
            // b.ProductionProgress01 = 0f;
        }

        // ---------------- conditions ----------------

        private static bool CanStartCycle(BuildingModel b, out StopReason reason)
        {
            // Outgoing storage must have space (incl. reservations)
            if (b.OutputStorage.FreeSpace <= 0)
            {
                reason = StopReason.OutputFull;
                return false;
            }

            // If recipe has inputs, they must exist in incoming storage
            var ok = true;
            reason = StopReason.None;

            b.Recipe.ForEachInput((rid, amount) =>
            {
                if (amount <= 0) return;
                if (b.InputStorage.Count(rid) < amount)
                    ok = false;
            });

            if (ok) return true;
            
            reason = StopReason.NoInput;
            return false;

        }

        private bool SchedulePullInputs(BuildingModel b)
        {
            // If no inputs, nothing to schedule
            if (!b.Recipe.HasInputs())
                return true;

            // Re-check inputs right now (can change since CanStartCycle)
            if (!HasAllInputsNow(b))
            {
                b.StopReason = StopReason.NoInput;
                return false;
            }

            // enqueue per unit transfers: InputStorage -> InPort
            var anyFailed = false;

            b.Recipe.ForEachInput((rid, amount) =>
            {
                for (int i = 0; i < amount; i++)
                {
                    var id = _scheduler.Enqueue(new TransferRequest(
                        Source: b.InputStorage,
                        Destination: b.InPort,
                        Resource: rid,
                        DurationSeconds: b.InputTransferSecondsPerUnit,
                        Tag: b.Id
                    ));

                    // if scheduler publishes Failed immediately, we detect it via Finished stream.
                    // But Enqueue always returns id; we still add route now.
                    _routes[id.Value] = new TransferRoute(b.Id, TransferKind.PullInput);
                    b.PendingPullTransfers++;
                }
            });

            // If scheduler couldn't start some tasks, it will emit Finished(Failed),
            // and OnTransferFinished will set TransferFailed. We'll handle that below:
            // (optional) proactive check:
            if (!anyFailed) return true;
            
            b.StopReason = StopReason.TransferBlocked;
            return false;

        }

        private bool SchedulePushOutput(BuildingModel b)
        {
            // ensure output storage has space now
            if (b.OutputStorage.FreeSpace <= 0)
            {
                b.StopReason = StopReason.OutputFull;
                return false;
            }

            // must have 1 output in OutPort
            if (b.OutPort.Count(b.Recipe.Output) <= 0)
            {
                b.StopReason = StopReason.TransferBlocked;
                return false;
            }

            var id = _scheduler.Enqueue(new TransferRequest(
                Source: b.OutPort,
                Destination: b.OutputStorage,
                Resource: b.Recipe.Output,
                DurationSeconds: b.OutputTransferSecondsPerUnit,
                Tag: b.Id
            ));

            _routes[id.Value] = new TransferRoute(b.Id, TransferKind.PushOutput);
            b.PendingPushTransfers++;

            return true;
        }

        private static bool HasAllInputsNow(BuildingModel b)
        {
            bool ok = true;
            b.Recipe.ForEachInput((rid, amount) =>
            {
                if (amount <= 0) return;
                if (b.InputStorage.Count(rid) < amount)
                    ok = false;
            });
            return ok;
        }

        private static bool ConsumeInputsFromInPort(BuildingModel b)
        {
            bool ok = true;
            b.Recipe.ForEachInput((rid, amount) =>
            {
                if (!b.InPort.TryConsume(rid, amount))
                    ok = false;
            });
            return ok;
        }

        private void OnTransferFinished(TransferFinished e)
        {
            if (!_routes.Remove(e.Id.Value, out var route))
                return;

            var b = FindBuilding(route.BuildingId);
            if (b == null) return;

            if (route.Kind == TransferKind.PullInput)
            {
                b.PendingPullTransfers = Math.Max(0, b.PendingPullTransfers - 1);
            }
            else
            {
                b.PendingPushTransfers = Math.Max(0, b.PendingPushTransfers - 1);
            }

            if (e.Status != TransferStatus.Failed) return;
            
            b.TransferFailed = true;

            // heuristic: decide stop reason based on current conditions
            b.StopReason = b.OutputStorage.FreeSpace <= 0 
                ? StopReason.OutputFull 
                : StopReason.TransferBlocked;

            // Optionally stop immediately:
            // Stop(b, b.StopReason);
        }

        private BuildingModel FindBuilding(BuildingId id)
        {
            return _buildings.FirstOrDefault(t => t.Id.Equals(id));
        }

        private readonly struct TransferRoute
        {
            public readonly BuildingId BuildingId;
            public readonly TransferKind Kind;

            public TransferRoute(BuildingId buildingId, TransferKind kind)
            {
                BuildingId = buildingId;
                Kind = kind;
            }
        }

        private enum TransferKind : byte
        {
            PullInput = 0,
            PushOutput = 1,
        }
    }
}
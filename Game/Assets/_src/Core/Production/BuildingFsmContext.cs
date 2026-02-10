using System;
using System.Collections.Generic;
using Game.Core.Common;
using Game.Core.Economy;
using Game.Core.Transfers;

namespace Game.Core.Production
{
    internal enum TransferKind : byte { PullInput, PushOutput }

    public sealed class BuildingFsmContext
    {
        public readonly BuildingModel B;
        public readonly ITransferScheduler Scheduler;

        // runtime
        public float ProduceTimer;
        public int PendingPull;
        public int PendingPush;
        public bool PullScheduled;
        public bool PushScheduled;

        // routing: transferId -> kind
        private readonly Dictionary<int, TransferKind> _routes = new();

        public BuildingFsmContext(BuildingModel building, ITransferScheduler scheduler)
        {
            B = building ?? throw new ArgumentNullException(nameof(building));
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        public void ResetCycleRuntime()
        {
            ProduceTimer = 0f;
            PendingPull = 0;
            PendingPush = 0;
            PullScheduled = false;
            PushScheduled = false;
            _routes.Clear();
        }

        public bool CanStartCycle(out StopReason reason)
        {
            if (B.OutputStorage.FreeSpace <= 0)
            {
                reason = StopReason.OutputFull;
                return false;
            }

            var ok = true;
            B.Recipe.ForEachInput((rid, amount) =>
            {
                if (amount <= 0) return;
                if (B.InputStorage.Count(rid) < amount) ok = false;
            });

            if (!ok)
            {
                reason = StopReason.NoInput;
                return false;
            }

            reason = StopReason.None;
            return true;
        }

        public bool SchedulePullInputs()
        {
            // якщо інпути зникли прямо зараз — стоп
            if (!HasAllInputsNow())
            {
                B.StopReason = StopReason.NoInput;
                return false;
            }

            PendingPull = 0;
            B.Recipe.ForEachInput((rid, amount) =>
            {
                for (var i = 0; i < amount; i++)
                {
                    var id = Scheduler.Enqueue(new TransferRequest(
                        Source: B.InputStorage,
                        Destination: B.InPort,
                        Resource: rid,
                        DurationSeconds: B.InputTransferSecondsPerUnit,
                        Tag: B.Id
                    ));

                    _routes[id.Value] = TransferKind.PullInput;
                    PendingPull++;
                }
            });

            return true;
        }

        public bool ConsumeInputsFromInPort()
        {
            var ok = true;
            B.Recipe.ForEachInput((rid, amount) =>
            {
                if (!B.InPort.TryConsume(rid, amount)) ok = false;
            });
            return ok;
        }

        public bool SpawnOutputToOutPort()
        {
            return B.OutPort.TryAddInstant(B.Recipe.Output);
        }

        public bool SchedulePushOutput()
        {
            if (B.OutputStorage.FreeSpace <= 0)
            {
                B.StopReason = StopReason.OutputFull;
                return false;
            }

            if (B.OutPort.Count(B.Recipe.Output) <= 0)
            {
                B.StopReason = StopReason.TransferBlocked;
                return false;
            }

            var id = Scheduler.Enqueue(new TransferRequest(
                Source: B.OutPort,
                Destination: B.OutputStorage,
                Resource: B.Recipe.Output,
                DurationSeconds: B.OutputTransferSecondsPerUnit,
                Tag: B.Id
            ));

            _routes[id.Value] = TransferKind.PushOutput;
            PendingPush = 1;
            return true;
        }

        public void OnTransferFinished(TransferFinished e)
        {
            if (!_routes.Remove(e.Id.Value, out var kind))
                return;

            if (kind == TransferKind.PullInput) PendingPull = Math.Max(0, PendingPull - 1);
            else PendingPush = Math.Max(0, PendingPush - 1);

            if (e.Status != TransferStatus.Failed) return;
            
            // просте визначення причини
            B.StopReason = B.OutputStorage.FreeSpace <= 0 
                ? StopReason.OutputFull 
                : StopReason.TransferBlocked;

            // Щоб state Tick() міг одразу зупинитись:
            if (kind == TransferKind.PullInput) PendingPull = -999;
            else PendingPush = -999;
        }

        private bool HasAllInputsNow()
        {
            var ok = true;
            B.Recipe.ForEachInput((rid, amount) =>
            {
                if (amount <= 0) return;
                if (B.InputStorage.Count(rid) < amount) ok = false;
            });
            return ok;
        }
    }
}
using System;
using Game.Core.Common;
using Game.Core.Common.Fsm;
using Game.Core.Production.States;
using Game.Core.Transfers;
using FTg.Common.Observables;

namespace Game.Core.Production
{
    /// <summary>
    /// Клас, який реалізує кінцевий автомат для керування станами будівлі, включаючи обробку переходів між станами та взаємодію з системою передач ресурсів.
    /// </summary>
    public sealed class BuildingFsm : ITickSystem, IDisposable
    {
        private readonly StateMachine<BuildingFsmContext, BuildingStatus> _sm;
        private readonly IDisposable _subFinished;

        public BuildingFsm(BuildingModel building, ITransferScheduler scheduler, ITransferStream stream)
        {
            var ctx = new BuildingFsmContext(building, scheduler);

            _sm = new StateMachine<BuildingFsmContext, BuildingStatus>(ctx)
                .Add(new BuildingStateIdle())
                .Add(new BuildingStatePullInputs())
                .Add(new BuildingStateProducing())
                .Add(new BuildingStatePushOutput())
                .Add(new BuildingStateStopped());

            _sm.Start(BuildingStatus.Idle);
            _subFinished = stream.Finished.Subscribe(ctx.OnTransferFinished);
        }

        public void Dispose() => _subFinished.Dispose();

        public void Tick(float dt) => _sm.Tick(dt);
    }
}
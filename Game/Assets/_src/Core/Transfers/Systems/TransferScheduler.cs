using System;
using System.Collections.Generic;
using FTg.Common.Observables;
using Game.Core.Common;
using Game.Core.Economy;

namespace Game.Core.Transfers
{
    /// <summary>
    /// Система планування передач ресурсів між контейнерами з урахуванням тривалості, прогресу та можливості скасування.
    /// </summary>
    public sealed class TransferScheduler : ITransferScheduler, ITransferStream
    {
        private int _nextId = 1;

        private readonly Dictionary<int, TaskImpl> _tasks = new();
        private readonly ObservableEvent<TransferStarted> _started = new();
        private readonly ObservableEvent<TransferProgress> _progress = new();
        private readonly ObservableEvent<TransferFinished> _finished = new();

        public IObservable<TransferStarted> Started => _started;
        public IObservable<TransferProgress> Progress => _progress;
        public IObservable<TransferFinished> Finished => _finished;

        public TransferId Enqueue(TransferRequest request)
        {
            if (request.Source == null) throw new ArgumentNullException(nameof(request.Source));
            if (request.Destination == null) throw new ArgumentNullException(nameof(request.Destination));

            var id = new TransferId(_nextId++);
            var duration = request.DurationSeconds <= 0f 
                ? 0.0001f 
                : request.DurationSeconds;

            // Резервуємо ресурс у джерела
            if (!request.Source.TryBeginRemove(request.Resource, out var removeToken))
            {
                // Не вдалося розпочати -> невдача
                PublishFailed(id);
                return id;
            }

            // Резервуємо місце в пункті призначення
            if (!request.Destination.TryReserveAdd(request.Resource, out var addReservation))
            {
                // Відкатуємо видалення
                request.Source.CancelRemove(removeToken);
                PublishFailed(id);
                return id;
            }

            // Створюємо task (статус: Виконується)
            var task = new TaskImpl(
                id: id,
                source: request.Source,
                destination: request.Destination,
                resource: request.Resource,
                durationSeconds: duration,
                tag: request.Tag,
                removeToken: removeToken,
                addReservation: addReservation
            );

            _tasks[id.Value] = task;

            // Повідомляємо про початок
            _started.Invoke(new TransferStarted(
                Id: id,
                Source: request.Source,
                Destination: request.Destination,
                Resource: request.Resource,
                DurationSeconds: duration,
                Tag: request.Tag
            ));

            // Якщо тривалість "миттєва", завершуємо одразу
            if (request.DurationSeconds <= 0f)
            {
                CompleteTask(task);
            }
            return id;
        }

        public bool TryGet(TransferId id, out ITransferTask task)
        {
            if (_tasks.TryGetValue(id.Value, out var impl))
            {
                task = impl;
                return true;
            }
            task = null;
            return false;
        }

        public void Cancel(TransferId id)
        {
            if (!_tasks.TryGetValue(id.Value, out var task))
                return;

            if (task.Status != TransferStatus.Running)
                return;

            CancelTask(task);
        }

        public void Tick(float dt)
        {
            if (dt < 0f) dt = 0f;

            // Знімок ітерації, щоб можна було модифікувати _tasks всередині (видалення/завершення)
            if (_tasks.Count == 0) return;

            var ids = new List<int>(_tasks.Keys);
            foreach (var key in ids)
            {
                if (!_tasks.TryGetValue(key, out var task))
                    continue;

                if (task.Status != TransferStatus.Running)
                    continue;

                task.ElapsedSeconds += dt;

                var p = task.ElapsedSeconds / task.DurationSeconds;
                if (p > 1f) p = 1f;

                task.Progress = p;

                // Публікуємо прогрес кожен тік (можна обмежити за бажанням)
                _progress.Invoke(new TransferProgress(task.Id, p));

                if (p >= 1f)
                {
                    CompleteTask(task);
                }
            }
        }

        private void CompleteTask(TaskImpl task)
        {
            if (task.Status != TransferStatus.Running)
                return;

            // Фіналізуємо додавання в пункті призначення (з джерела вже видалено при постановці в чергу)
            task.Destination.CommitAdd(task.AddReservation);
            task.Status = TransferStatus.Completed;
            task.Progress = 1f;

            _finished.Invoke(new TransferFinished(task.Id, TransferStatus.Completed));
            _tasks.Remove(task.Id.Value);
        }

        private void CancelTask(TaskImpl task)
        {
            if (task.Status != TransferStatus.Running)
                return;

            // Відкатуємо обидві резервації
            task.Source.CancelRemove(task.RemoveToken);
            task.Destination.CancelAdd(task.AddReservation);

            task.Status = TransferStatus.Cancelled;

            _finished.Invoke(new TransferFinished(task.Id, TransferStatus.Cancelled));
            _tasks.Remove(task.Id.Value);
        }

        private void PublishFailed(TransferId id)
        {
            _finished.Invoke(new TransferFinished(id, TransferStatus.Failed));
        }

        #region TaskImpl
        private sealed class TaskImpl : ITransferTask
        {
            public TransferId Id { get; }
            public ResourceId Resource { get; }
            public float DurationSeconds { get; }
            public float Progress { get; internal set; }
            public TransferStatus Status { get; internal set; }

            public float ElapsedSeconds { get; internal set; }

            public IResourceContainer Source { get; }
            public IResourceContainer Destination { get; }
            public object Tag { get; }

            public RemoveToken RemoveToken { get; }
            public AddReservation AddReservation { get; }

            public TaskImpl(
                TransferId id,
                IResourceContainer source,
                IResourceContainer destination,
                ResourceId resource,
                float durationSeconds,
                object tag,
                RemoveToken removeToken,
                AddReservation addReservation)
            {
                Id = id;
                Source = source;
                Destination = destination;
                Resource = resource;
                DurationSeconds = durationSeconds;
                Tag = tag;

                RemoveToken = removeToken;
                AddReservation = addReservation;

                Status = TransferStatus.Running;
                Progress = 0f;
                ElapsedSeconds = 0f;
            }
        }
        #endregion
    }
}

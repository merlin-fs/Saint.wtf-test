namespace Game.Core.Common.Fsm
{
    /// <summary>
    /// Стан автомата.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TStateId"></typeparam>
    public interface IState<in TContext, TStateId>
    {
        TStateId Id { get; }

        /// <summary>
        /// Викликається при вході в стан. Можна використовувати для ініціалізації, запуску анімацій, тощо.
        /// </summary>
        /// <param name="ctx"></param>
        void Enter(TContext ctx);
        /// <summary>
        /// Викликається при виході зі стану. Можна використовувати для очищення, зупинки анімацій, тощо.
        /// </summary>
        /// <param name="ctx"></param>
        void Exit(TContext ctx);

        /// <summary>
        /// Повертає значення true, якщо стан запитує перехід до наступного стану.
        /// </summary>
        bool Tick(TContext ctx, float dt, out TStateId next);
    }
}
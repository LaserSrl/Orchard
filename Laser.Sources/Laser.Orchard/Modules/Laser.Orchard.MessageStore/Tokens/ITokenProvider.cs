using Orchard.Events;

namespace Laser.Orchard.MessageStore.Tokens {
    public interface ITokenProvider : IEventHandler {
        void Describe(dynamic context);
        void Evaluate(dynamic context);
    }
}
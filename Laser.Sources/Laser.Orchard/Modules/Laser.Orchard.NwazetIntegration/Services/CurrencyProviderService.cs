using Nwazet.Commerce.Services;
using Orchard;

namespace Laser.Orchard.NwazetIntegration.Services {
    public class CurrencyProviderService : CurrencyProviderBase {
        public CurrencyProviderService(IWorkContextAccessor workContextAccessor) : base(workContextAccessor) {
        }

        public override string Name
        {
            get
            {
                return "USD";
            }
        }
        public override string Description
        {
            get
            {
                return "USD";
            }
        }
        protected override Currency SelectedCurrency
        {
            get
            {
                return new Currency("USD", "USD", "$", 2);
            }
        }
    }
}
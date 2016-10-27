using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;
using Orchard.Data;
using System;
using Laser.Orchard.OpenAuthentication.ViewModels;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IProviderConfigurationService : IDependency {
        IEnumerable<ProviderConfigurationRecord> GetAll();
        ProviderConfigurationRecord Get(string providerName);
        void Delete(int id);
        void Create(ProviderConfigurationCreateParams parameters);
        bool VerifyUnicity(string providerName);
        bool VerifyUnicity(string providerName, int id);
        CreateProviderViewModel Get(Int32 id);
        void Edit(CreateProviderViewModel parameters);
    }

    public class ProviderConfigurationService : IProviderConfigurationService {
        private readonly IRepository<ProviderConfigurationRecord> _repository;

        public ProviderConfigurationService(IRepository<ProviderConfigurationRecord> repository) {
            _repository = repository;
        }

        public IEnumerable<ProviderConfigurationRecord> GetAll() {
            return _repository.Table.ToList();
        }

        public ProviderConfigurationRecord Get(string providerName) {
            return _repository.Fetch(o => o.ProviderName == providerName).FirstOrDefault();
           //return _repository.Get(o => o.ProviderName == providerName);
        }

        public void Delete(int id) {
            _repository.Delete(_repository.Get(o => o.Id == id));
        }

        public bool VerifyUnicity(string providerName) {
            return _repository.Get(o => o.ProviderName == providerName) == null;
        }
        public bool VerifyUnicity(string providerName,int id) {
            return _repository.Get(o => o.ProviderName == providerName && o.Id!=id) == null;
        }

        public void Create(ProviderConfigurationCreateParams parameters) {
            _repository.Create(new ProviderConfigurationRecord {
                DisplayName = parameters.DisplayName,
                ProviderName = parameters.ProviderName,
                ProviderIdentifier = parameters.ProviderIdentifier,
                UserIdentifier=parameters.UserIdentifier,
                ProviderIdKey = parameters.ProviderIdKey,
                ProviderSecret = parameters.ProviderSecret,
                IsEnabled = 1
            });
        }

        public void Edit(CreateProviderViewModel parameters) {
            ProviderConfigurationRecord rec = _repository.Get(parameters.Id);
            rec.DisplayName = parameters.DisplayName;
            rec.IsEnabled = parameters.IsEnabled?1:0;
            rec.ProviderIdentifier = parameters.ProviderIdentifier;
            rec.UserIdentifier = parameters.UserIdentifier;
            rec.ProviderIdKey = parameters.ProviderIdKey;
            rec.ProviderName = parameters.ProviderName;
            rec.ProviderSecret = parameters.ProviderSecret;
            _repository.Update(rec);
       
        }


        public CreateProviderViewModel Get(Int32 id) {
            CreateProviderViewModel cpvm = new CreateProviderViewModel();
            ProviderConfigurationRecord prec = _repository.Get(o => o.Id == id);
            cpvm.Id = prec.Id;
            cpvm.DisplayName = prec.DisplayName;
            cpvm.IsEnabled = prec.IsEnabled==1;
            cpvm.ProviderIdentifier = prec.ProviderIdentifier;
            cpvm.UserIdentifier = prec.UserIdentifier;
            cpvm.ProviderIdKey = prec.ProviderIdKey;
            cpvm.ProviderName = prec.ProviderName;
            cpvm.ProviderSecret = prec.ProviderSecret;


            return cpvm;

        }
    }

    public class ProviderConfigurationCreateParams {
        public string DisplayName { get; set; }
        public string ProviderName { get; set; }
        public string ProviderIdentifier { get; set; }
        public string UserIdentifier { get; set; }
        public string ProviderIdKey { get; set; }
        public string ProviderSecret { get; set; }
    }
}
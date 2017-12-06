using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFApplication : SFBase
    {
        private SFRoot _parent;
        private Application _application;

        public SFRoot Parent { get { return _parent; } }
        public Application Properties { get { return _application; } }
        public List<SFService> Services { get; } = new List<SFService>();
        public ApplicationLoadInformation LoadInformation { get; private set; }
        

        public SFApplication(FabricClient fabricClient, SFRoot parent, Application application)
            : base(fabricClient)
        {
            _parent = parent;
            _application = application;
        }

        protected override async Task OnEnumChildren(bool deep)
        {
            Services.Clear();
            var services = await _fabricClient.QueryManager.GetServiceListAsync(_application.ApplicationName);
            foreach (var service in services)
            {
                var sfService = new SFService(_fabricClient, this, service);
                Services.Add(sfService);
                if (deep)
                    await sfService.EnumChildren(deep);
            }

            LoadInformation = await _fabricClient.QueryManager.GetApplicationLoadInformationAsync(_application.ApplicationName.AbsolutePath);
        }
    }
}

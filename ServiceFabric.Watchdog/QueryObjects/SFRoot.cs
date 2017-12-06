using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFRoot : SFBase
    {
        public List<SFApplication> Applications { get; } = new List<SFApplication>();
        public List<SFNode> Nodes { get; } = new List<SFNode>();
        public ClusterLoadInformation LoadInformation { get; private set; }

        public SFRoot(FabricClient fabricClient)
            : base(fabricClient)
        {
        }

        protected async override Task OnEnumChildren(bool deep)
        {
            Applications.Clear();
            var applications = await _fabricClient.QueryManager.GetApplicationListAsync();
            foreach (var application in applications)
            {
                var sfApplication = new SFApplication(_fabricClient, this, application);
                Applications.Add(sfApplication);
                if (deep)
                    await sfApplication.EnumChildren(deep);
            }

            Nodes.Clear();
            var nodes = await _fabricClient.QueryManager.GetNodeListAsync();
            foreach (var node in nodes)
            {
                var sfNode = new SFNode(_fabricClient, this, node);
                Nodes.Add(sfNode);
                if (deep)
                    await sfNode.EnumChildren(deep);
            }

            LoadInformation = await _fabricClient.QueryManager.GetClusterLoadInformationAsync();
        }
    }
}

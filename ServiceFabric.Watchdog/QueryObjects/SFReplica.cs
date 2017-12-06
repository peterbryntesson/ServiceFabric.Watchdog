using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFReplica : SFBase
    {
        private SFPartition _parent;
        private Replica _replica;

        public SFPartition Parent { get { return _parent; } }
        public Replica Properties {  get { return _replica; } }
        public DeployedServiceReplicaDetail DeploymentDetails { get; private set; } = null;

        public SFReplica(FabricClient fabricClient, SFPartition parent, Replica replica)
            : base(fabricClient)
        {
            _parent = parent;
            _replica = replica;
        }

        protected async override Task OnEnumChildren(bool deep)
        {
            DeploymentDetails = await _fabricClient.QueryManager.GetDeployedReplicaDetailAsync(_replica.NodeName, 
                                                    _parent.Properties.PartitionInformation.Id, _replica.Id);
        }
    }
}

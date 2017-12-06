using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFPartition : SFBase
    {
        private SFService _parent;
        private Partition _partition;

        public SFService Parent { get { return _parent; } }
        public Partition Properties { get { return _partition; } }
        public List<SFReplica> Replicas { get; } = new List<SFReplica>();
        public PartitionLoadInformation Loadinformation { get; private set; }

        public SFPartition(FabricClient fabricClient, SFService parent, Partition partition)
            : base(fabricClient)
        {
            _parent = parent;
            _partition = partition;
        }

        protected override async Task OnEnumChildren(bool deep)
        {
            var replicas = await _fabricClient.QueryManager.GetReplicaListAsync(_partition.PartitionInformation.Id);
            foreach (var replica in replicas)
            {
                var sfReplica = new SFReplica(_fabricClient, this, replica);
                Replicas.Add(sfReplica);
                if (deep)
                    await sfReplica.EnumChildren(deep);
            }

            Loadinformation = await _fabricClient.QueryManager.GetPartitionLoadInformationAsync(_partition.PartitionInformation.Id);
        }
    }
}

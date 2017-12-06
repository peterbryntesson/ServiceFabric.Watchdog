using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace ServiceFabric.Watchdog.QueryObjects
{
    public class SFService : SFBase
    {
        private Service _service;
        private SFApplication _parent;

        public SFApplication Parent { get { return _parent; } }
        public Service Properties { get { return _service; } }
        public List<SFPartition> Partitions { get; } = new List<SFPartition>();

        public SFService(FabricClient fabricClient, SFApplication parent, Service service)
            : base(fabricClient)
        {
            _parent = parent;
            _service = service;
        }

        protected override async Task OnEnumChildren(bool deep)
        {
            Partitions.Clear();
            var partitions = await _fabricClient.QueryManager.GetPartitionListAsync(_service.ServiceName);
            foreach (var partition in partitions)
            {
                var sfService = new SFPartition(_fabricClient, this, partition);
                Partitions.Add(sfService);
                if (deep)
                    await sfService.EnumChildren(deep);
            }
        }
    }
}

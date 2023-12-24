using BackendProject.Horizon.RT.Common;

namespace BackendProject.Horizon.LIBRARY.Database.Models
{
    public partial class DimLocations
    {
        public int Id { get; set; }

        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? ChannelName { get; set; }
        public int[]? AppIds { get; set; }
        public MediusWorldGenericFieldLevelType GenericFieldLevel { get; set; }

        public DateTime CreateDt { get; set; }
        public DateTime? ModifiedDt { get; set; }
        public DateTime FromDt { get; set; }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;

namespace backend.Entities
{
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssignmentId { get; set; }
        public int AssignedToUserId { get; set; }
        public int AssignedByUserId { get; set; }
        public int AssetId { get; set; }
        public Asset Asset { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Note { get; set; }
        public virtual User AssignedTo { get; set; }
        public virtual User AssignedBy { get; set; }
        [Required, DefaultValue(AssetState.WaitingForRecycle)]
        public AssignmentState AssignmentState { get; set; }
    }
}
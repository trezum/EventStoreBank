using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Checkpoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public ulong CommitPosition { get; set; }
        public ulong PreparePosition { get; set; }
    }
}

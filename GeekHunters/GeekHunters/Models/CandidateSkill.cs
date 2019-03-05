using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekHunters.Models
{
    [Table("CandidateSkill")]
    public class CandidateSkill
    {
        public long CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public long SkillId { get; set; }
        public Skill Skill { get; set; }
    }
}

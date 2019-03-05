using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GeekHunters.Models;

namespace GeekHunters.ViewModel
{
    public class CandidateFormViewModel
    {
        public IEnumerable<Skill> Skills { get; set; }
        public Candidate candidate { get; set; }

        public List<long> chosenSkills { get; set; } = new List<long>();

    }
}

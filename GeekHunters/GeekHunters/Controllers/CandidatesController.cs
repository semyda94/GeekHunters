using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using GeekHunters.Data;
using GeekHunters.Models;
using GeekHunters.ViewModel;

namespace GeekHunters.Controllers
{
    public class CandidatesController : Controller
    {
        private GeekHuntersDbContext _context;

        public CandidatesController(GeekHuntersDbContext context)
        {
            _context = context;
        }

        public IActionResult PresentCandidateTable(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            List<Candidate> candidates;
            //Gathering list of candidates based on the search field
            if (!String.IsNullOrEmpty(searchString))
            {
                candidates = _context.Candidates.Include(c => c.CandidateSkills).ThenInclude(c => c.Skill).Where(c => c.FirstName.ToLower().Contains(searchString.ToLower()) || c.LastName.ToLower().Contains(searchString.ToLower()) || c.CandidateSkills.Where(cs => cs.Skill.Name.ToLower().Contains(searchString.ToLower())).Any()).ToList();
            } else
            {
                candidates = _context.Candidates.Include(c => c.CandidateSkills).ThenInclude(c => c.Skill).ToList();
            }

            return View(candidates);
        }

        public IActionResult CandidatePresentation(int id)
        {
            var viewModel = new CandidateFormViewModel();

            viewModel.Skills = _context.Skills.ToList();
            //Case when button "Add new Candidate" has been clicked it will be passed id parametr -1
            if (id == -1)
            {
                viewModel.candidate = new Candidate() { Id = -1 };
                return View(viewModel);
            }

            //Otherwise "Edit" button was clicked and we passing information reagrding chossen Candidate
            viewModel.candidate = _context.Candidates.SingleOrDefault(c => c.Id == id);

            //In case if Couldn't find a candidated with passed id present not found page
            if (viewModel.candidate == null)
                return NotFound();

            var candidateChoosenSkills = _context.CandidateSkills.Where(cs => cs.CandidateId == viewModel.candidate.Id).ToList();
            
            foreach (var candidateSkill in candidateChoosenSkills)
            {
                viewModel.chosenSkills.Add(candidateSkill.SkillId);
            }

            return View(viewModel); ;
        }

        [HttpPost]
        public IActionResult Save(CandidateFormViewModel viewModel)
        {
            //Check the validation is passed base on the anotations at Model Class
            if (!ModelState.IsValid)
            {
                var InvalidViewModel = new CandidateFormViewModel
                {
                    candidate = viewModel.candidate,
                    Skills = _context.Skills.ToList(),
                    chosenSkills = viewModel.chosenSkills
                };
                return View("CandidatePresentation", InvalidViewModel);
            }
                
            //Creating a new Candidate
            if (viewModel.candidate.Id == -1)
            {

                var newCandidate = new Candidate { FirstName = viewModel.candidate.FirstName, LastName = viewModel.candidate.LastName };

                
                foreach (var skillId in viewModel.chosenSkills)
                {

                    newCandidate.CandidateSkills.Add(new CandidateSkill { Candidate = viewModel.candidate, Skill = _context.Skills.Single(s => s.Id == skillId) });
                }
                _context.Candidates.Add(newCandidate);

            } else
            {
                //Updating existing one
                var updateCandidate = _context.Candidates.Include(c => c.CandidateSkills).ThenInclude(c => c.Skill).Single(c => c.Id == viewModel.candidate.Id);
                updateCandidate.FirstName = viewModel.candidate.FirstName;
                updateCandidate.LastName = viewModel.candidate.LastName;

                foreach (var candidateskill in updateCandidate.CandidateSkills.ToList())
                {
                    if (viewModel.chosenSkills.Contains(candidateskill.SkillId))
                    {
                        viewModel.chosenSkills.Remove(candidateskill.SkillId);
                    } else
                    {
                        updateCandidate.CandidateSkills.Remove(candidateskill);
                    }
                }

                foreach (var skillId in viewModel.chosenSkills)
                {
                    updateCandidate.CandidateSkills.Add(new CandidateSkill {Candidate = viewModel.candidate, Skill = _context.Skills.Single(s => s.Id == skillId) });
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return RedirectToAction("PresentCandidateTable", "Candidates");
        }

        public IActionResult Delete(int id)
        {
            //At this action checking of getted id not required
            var candidateToRemove = _context.Candidates.Single(c => c.Id == id);

            _context.Candidates.Remove(candidateToRemove);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return RedirectToAction("PresentCandidateTable", "Candidates");
        }
    }
}
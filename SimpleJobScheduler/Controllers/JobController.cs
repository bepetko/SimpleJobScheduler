using Microsoft.AspNetCore.Mvc;
using SimpleJobScheduler.Data;
using SimpleJobScheduler.Models.Domain;
using SimpleJobScheduler.Models;
using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace SimpleJobScheduler.Controllers
{
    public class JobController : Controller
    {
        private readonly JobSchedulerDbContext _context;

        public JobController(JobSchedulerDbContext context) 
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["successMessage"] as string;
            ViewBag.FailureMessage = TempData["failureMessage"] as string;
            var jobs = await _context.Jobs.ToListAsync();
            var jobsToShow = new List<JobsToShowViewModel>();
            foreach (var job in jobs)
            {
                var jobLastExecution = await _context.JobsHistory.Where(x => x.JobId == job.Id).OrderByDescending(x => x.ExecutionDate).FirstOrDefaultAsync();
                jobsToShow.Add(new JobsToShowViewModel() {
                    Id = job.Id,
                    LastExecutionTime = job.DateOfLastStart,
                    Name = job.Name,
                    Result = jobLastExecution?.Response,
                    NextExecutionTime = job.DateOfLastStart?.AddMinutes(job.IntervalInMinutes)
                });
            }
            return View(jobsToShow);
        }

        [HttpGet]
        public IActionResult AddJob()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddJob(AddJobViewModel addJobRequest)
        {
            if (ModelState.IsValid)
            {
                var job = new Job()
                {
                    Id = Guid.NewGuid(),
                    Name = addJobRequest.Name,
                    Url = addJobRequest.Url,
                    Description = addJobRequest.Description,
                    IntervalInMinutes = addJobRequest.IntervalInMinutes,
                    DateOfLastStart = null
                };

                await _context.Jobs.AddAsync(job);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> JobUpdate(Guid id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            var viewModel = new JobUpdateViewModel()
            {
                Id = job.Id,
                Name = job.Name,
                Url = job.Url,
                Description = job.Description,
                Interval = job.IntervalInMinutes,
                DateOfLastStart = job.DateOfLastStart
            };

            return await Task.Run(() => View("JobUpdate", viewModel));
        }


        [HttpPost]
        public async Task<IActionResult> JobUpdate(JobUpdateViewModel model)
        {
            var job = await _context.Jobs.FindAsync(model.Id);

            if (job != null)
            {
                job.Name = model.Name;
                job.Description = model.Description;
                job.IntervalInMinutes = model.Interval;
                job.Url = model.Url;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(JobUpdateViewModel model)
        {
            var job = await _context.Jobs.FindAsync(model.Id);

            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Start(Guid id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var httpClient = new HttpClient();
            var url = job.Url;
            job.DateOfLastStart = DateTime.Now;

            try
            {
                var response = await httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var jobExecutionHistory = new JobsHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = statusCode,
                    Response = responseBody,
                    JobId = job.Id,
                    Success = response.IsSuccessStatusCode
                };
                _context.JobsHistory.Add(jobExecutionHistory);
                await _context.SaveChangesAsync();

                var message = $"Job '{job.Name}' was executed successfuly!";
                TempData["successMessage"] = message;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                // connectivity error
                var jobExecutionHistory = new JobsHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = 0, // Set status code to 0 for connection failure
                    Response = $"Connectivity error! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                _context.JobsHistory.Add(jobExecutionHistory);
                await _context.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }
            catch (HttpRequestException ex) when (ex.InnerException is null)
            {
                var jobExecutionHistory = new JobsHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Response = $"Url malformed! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                _context.JobsHistory.Add(jobExecutionHistory);
                await _context.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }
            catch (Exception ex)
            {
                var jobExecutionHistory = new JobsHistory
                {
                    ExecutionDate = DateTime.Now,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Response = $"Exception thrown! [{ex.Message}]",
                    JobId = job.Id,
                    Success = false
                };
                _context.JobsHistory.Add(jobExecutionHistory);
                await _context.SaveChangesAsync();

                var message = $"Job '{job.Name}' failed!";
                TempData["failureMessage"] = message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> JobHistory(Guid id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            var history = await _context.JobsHistory
                .Where(x => x.JobId == id).OrderByDescending(x => x.ExecutionDate).Take(10).ToListAsync();

            var historyModel = new JobHistoryViewModel()
            {
                JobId = job.Id,
                JobName = job.Name,
                JobExecutionHistoryList = history.Select(x => new JobExecutionHistoryViewModel()
                {
                    ExecutionDate = x.ExecutionDate,
                    StatusCode = x.StatusCode,
                    Response = x.Response,
                    Success = x.Success
                }).ToList()
            };

            return View(historyModel);
        }
    }
}


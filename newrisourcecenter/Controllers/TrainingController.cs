using Microsoft.AspNet.Identity;
using newrisourcecenter.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace newrisourcecenter.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        private readonly RisourceCenterMexicoEntities db = new RisourceCenterMexicoEntities();
        private readonly RisourceCenterContext identityDb = new RisourceCenterContext();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public ActionResult Admin()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public ActionResult AdminList()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public ActionResult AdminEdit(int id)
        {
            ViewBag.TrainingId = id;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public ActionResult AdminReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> GetAllTrainingsAdmin()
        {
            var trainings = await db.TrainingContents
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    PassingPercentage = x.PassingPercentage ?? 0,
                    HasPdf = x.PdfPath != null && x.PdfPath != "",
                    HasVideo = x.VideoPath != null && x.VideoPath != "",
                    QuestionCount = x.QuizQuestions.Count(),
                    RoleNames = x.TrainingRoleAssignments.Select(a => a.AspNetRole.Name)
                })
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Json(trainings, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> GetTrainingStatsAdmin(int trainingContentId)
        {
            var trainingExists = await db.TrainingContents.AnyAsync(x => x.Id == trainingContentId);
            if (!trainingExists)
            {
                Response.StatusCode = 404;
                return Json("Training not found.", JsonRequestBehavior.AllowGet);
            }

            var progresses = await db.UserProgresses
                .Where(x => x.TrainingContentId == trainingContentId)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.StartTime,
                    x.EndTime,
                    x.ScorePercentage,
                    x.IsPassed
                })
                .ToListAsync();

            var userIds = progresses.Select(x => x.UserId).Distinct().ToList();

            var users = await db.usr_user
                .Where(x => userIds.Contains(x.usr_ID))
                .Select(x => new { x.usr_ID, x.usr_fName, x.usr_lName, x.usr_email })
                .ToListAsync();

            var userById = users.ToDictionary(x => x.usr_ID, x => x);

            var attempts = await db.TrainingQuizAttempts
                .Where(x => x.TrainingContentId == trainingContentId)
                .Select(x => new { x.UserId, x.SubmittedAt, x.ScorePercentage, x.IsPassed })
                .ToListAsync();

            var latestAttemptByUser = attempts
                .GroupBy(x => x.UserId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).FirstOrDefault())
                .Where(x => x != null)
                .ToDictionary(x => x.UserId, x => x);

            var nowUtc = DateTime.UtcNow;
            var rows = progresses
                .OrderByDescending(x => x.StartTime)
                .Select(p =>
                {
                    var u = userById.ContainsKey(p.UserId) ? userById[p.UserId] : null;
                    var end = p.EndTime ?? nowUtc;
                    var elapsed = end - p.StartTime;

                    object attempt;
                    if (!latestAttemptByUser.TryGetValue(p.UserId, out var last))
                    {
                        attempt = null;
                    }
                    else
                    {
                        attempt = new { last.SubmittedAt, last.ScorePercentage, last.IsPassed };
                    }

                    return new
                    {
                        p.Id,
                        p.UserId,
                        UserName = u == null ? "" : (u.usr_fName + " " + u.usr_lName),
                        UserEmail = u == null ? "" : u.usr_email,
                        p.StartTime,
                        p.EndTime,
                        ElapsedSeconds = Math.Max(0, (int)elapsed.TotalSeconds),
                        LatestAttempt = attempt,
                        ProgressScorePercentage = p.ScorePercentage,
                        ProgressIsPassed = p.IsPassed
                    };
                })
                .ToList();

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAssignedTrainings()
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                Response.StatusCode = 401;
                return Json("Please Login. Login has timed out", JsonRequestBehavior.AllowGet);
            }

            string identityUserId = User.Identity.GetUserId();
            var userRoleIds = await identityDb.Users
                .Where(x => x.Id == identityUserId)
                .SelectMany(x => x.Roles.Select(r => r.RoleId))
                .ToListAsync();

            if (userRoleIds.Count == 0)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var allTrainings = await db.TrainingContents.ToListAsync();
            var trainingIds = allTrainings.Select(x => x.Id).ToList();
            var roleAssignments = await GetAssignmentsByTrainingIds(trainingIds);

            var startedTrainingIds = await db.UserProgresses
                .Where(x => x.UserId == (int)userId && x.EndTime == null && x.TrainingContentId.HasValue)
                .Select(x => x.TrainingContentId.Value)
                .Distinct()
                .ToListAsync();

            var startedSet = new HashSet<int>(startedTrainingIds);

            var assignedTrainings = allTrainings
                .Where(x => roleAssignments.ContainsKey(x.Id) && roleAssignments[x.Id].Intersect(userRoleIds).Any())
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.PdfPath,
                    x.VideoPath,
                    PassingPercentage = x.PassingPercentage ?? 0,
                    IsStarted = startedSet.Contains(x.Id)
                })
                .OrderBy(x => x.Title)
                .ToList();

            return Json(assignedTrainings, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> StartTraining(int trainingContentId)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                Response.StatusCode = 401;
                return Json("Please Login. Login has timed out");
            }

            var training = await db.TrainingContents.FindAsync(trainingContentId);
            if (training == null)
            {
                Response.StatusCode = 404;
                return Json("Training not found");
            }

            var progress = await db.UserProgresses
                .Where(x => x.UserId == (int)userId && x.TrainingContentId == trainingContentId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = (int)userId,
                    TrainingContentId = trainingContentId,
                    StartTime = DateTime.UtcNow
                };
                db.UserProgresses.Add(progress);
                await db.SaveChangesAsync();
            }
            else
            {
                progress.StartTime = DateTime.UtcNow;
                progress.EndTime = null;
                progress.ScorePercentage = null;
                progress.IsPassed = null;
                await db.SaveChangesAsync();
            }

            return Json(new { progressId = progress.Id, startedAt = progress.StartTime });
        }

        [HttpGet]
        public async Task<ActionResult> OpenContent(int trainingContentId, string contentType)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                return new HttpStatusCodeResult(401, "Please login.");
            }

            var training = await db.TrainingContents.FindAsync(trainingContentId);
            if (training == null)
            {
                return HttpNotFound("Training not found.");
            }

            var activeProgress = await db.UserProgresses
                .Where(x => x.UserId == (int)userId && x.TrainingContentId == trainingContentId && x.EndTime == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (activeProgress == null)
            {
                return new HttpStatusCodeResult(400, "Please start training first.");
            }

            string contentPath = null;
            if (string.Equals(contentType, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                contentPath = training.PdfPath;
            }
            else if (string.Equals(contentType, "video", StringComparison.OrdinalIgnoreCase))
            {
                contentPath = training.VideoPath;
            }

            if (string.IsNullOrWhiteSpace(contentPath))
            {
                return new HttpStatusCodeResult(404, "Requested content is not available.");
            }

            return Redirect(contentPath);
        }

        [HttpGet]
        public async Task<JsonResult> GetQuiz(int trainingContentId)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                Response.StatusCode = 401;
                return Json("Please Login. Login has timed out", JsonRequestBehavior.AllowGet);
            }

            var activeProgress = await db.UserProgresses
                .Where(x => x.UserId == (int)userId && x.TrainingContentId == trainingContentId && x.EndTime == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (activeProgress == null)
            {
                Response.StatusCode = 400;
                return Json("Please start training before attempting the quiz.", JsonRequestBehavior.AllowGet);
            }

            var quiz = await db.QuizQuestions
                .Where(x => x.TrainingContentId == trainingContentId)
                .Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    Options = q.QuizOptions.Select(o => new { o.Id, o.OptionText })
                })
                .ToListAsync();

            return Json(quiz, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SubmitQuiz(QuizSubmissionViewModel submission)
        {
            long userId = Convert.ToInt64(Session["userId"]);
            if (!Request.IsAuthenticated || userId == 0)
            {
                Response.StatusCode = 401;
                return Json("Please Login. Login has timed out");
            }

            if (submission == null || submission.Answers == null || submission.Answers.Count == 0)
            {
                Response.StatusCode = 400;
                return Json("Please submit at least one answer.");
            }

            var training = await db.TrainingContents.FindAsync(submission.ContentId);
            if (training == null)
            {
                Response.StatusCode = 404;
                return Json("Training not found.");
            }

            var progress = await db.UserProgresses
                .Where(x => x.UserId == (int)userId && x.TrainingContentId == submission.ContentId && x.EndTime == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (progress == null)
            {
                Response.StatusCode = 400;
                return Json("Please start training before submitting the quiz.");
            }

            var trainingQuestionIds = await db.QuizQuestions
                .Where(x => x.TrainingContentId == submission.ContentId)
                .Select(x => x.Id)
                .ToListAsync();

            int totalQuestions = trainingQuestionIds.Count;
            if (totalQuestions == 0)
            {
                Response.StatusCode = 400;
                return Json("No quiz configured for this training.");
            }

            var correctOptions = await db.QuizOptions
                .Where(x => x.IsCorrect && x.QuestionId.HasValue && trainingQuestionIds.Contains(x.QuestionId.Value))
                .Select(x => new { QuestionId = x.QuestionId.Value, x.Id })
                .ToListAsync();

            var correctOptionIdsByQuestion = correctOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<int>(g.Select(x => x.Id)));

            var selectedOptionIdsByQuestion = submission.Answers
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    g => g.Key,
                    g => new HashSet<int>(
                        g.SelectMany(x => x.SelectedOptionIds ?? new List<int>()).Distinct()));

            int correctCount = 0;
            foreach (int questionId in trainingQuestionIds)
            {
                HashSet<int> correctSet;
                if (!correctOptionIdsByQuestion.TryGetValue(questionId, out correctSet))
                {
                    continue;
                }

                HashSet<int> selectedSet;
                if (!selectedOptionIdsByQuestion.TryGetValue(questionId, out selectedSet))
                {
                    continue;
                }

                if (selectedSet.SetEquals(correctSet))
                {
                    correctCount++;
                }
            }

            int scorePercentage = (int)Math.Round((correctCount / (double)totalQuestions) * 100, MidpointRounding.AwayFromZero);
            int passPercentage = training.PassingPercentage ?? 0;
            bool isPassed = scorePercentage >= passPercentage;

            var submittedAtUtc = DateTime.UtcNow;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var attempt = new TrainingQuizAttempt
                    {
                        TrainingContentId = submission.ContentId,
                        UserId = (int)userId,
                        UserProgressId = progress.Id,
                        StartedAt = progress.StartTime,
                        SubmittedAt = submittedAtUtc,
                        TotalQuestions = totalQuestions,
                        CorrectQuestions = correctCount,
                        ScorePercentage = scorePercentage,
                        PassingPercentage = passPercentage,
                        IsPassed = isPassed
                    };

                    foreach (var kvp in selectedOptionIdsByQuestion)
                    {
                        int questionId = kvp.Key;
                        var selectedIds = kvp.Value;

                        HashSet<int> correctSet;
                        correctOptionIdsByQuestion.TryGetValue(questionId, out correctSet);

                        foreach (int optionId in selectedIds)
                        {
                            attempt.TrainingQuizAttemptAnswers.Add(new TrainingQuizAttemptAnswer
                            {
                                QuestionId = questionId,
                                OptionId = optionId,
                                IsCorrect = correctSet != null && correctSet.Contains(optionId)
                            });
                        }
                    }

                    db.TrainingQuizAttempts.Add(attempt);

                    progress.EndTime = submittedAtUtc;
                    progress.ScorePercentage = scorePercentage;
                    progress.IsPassed = isPassed;

                    await db.SaveChangesAsync();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            TimeSpan elapsed = progress.EndTime.Value - progress.StartTime;
            return Json(new
            {
                scorePercentage,
                passPercentage,
                isPassed,
                elapsedSeconds = Math.Max(0, (int)elapsed.TotalSeconds)
            });
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> GetAdminTrainingDetails(int id)
        {
            bool hasAttempts = await db.TrainingQuizAttempts.AnyAsync(x => x.TrainingContentId == id);

            var training = await db.TrainingContents
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.PdfPath,
                    x.VideoPath,
                    PassingPercentage = x.PassingPercentage ?? 0,
                    Questions = x.QuizQuestions.Select(q => new
                    {
                        q.Id,
                        q.QuestionText,
                        Options = q.QuizOptions.Select(o => new { o.Id, o.OptionText, o.IsCorrect })
                    })
                })
                .FirstOrDefaultAsync();

            if (training == null)
            {
                Response.StatusCode = 404;
                return Json("Training not found.", JsonRequestBehavior.AllowGet);
            }

            var roleIds = await db.TrainingRoleAssignments
                .Where(x => x.TrainingContentId == id)
                .Select(x => x.RoleId)
                .ToListAsync();

            return Json(new { training, roleIds, hasAttempts }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> Update(TrainingUpdateViewModel model)
        {
            if (model == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid payload.");
            }

            var training = await db.TrainingContents.FindAsync(model.Id);
            if (training == null)
            {
                Response.StatusCode = 404;
                return Json("Training not found.");
            }

            bool hasAttempts = await db.TrainingQuizAttempts.AnyAsync(x => x.TrainingContentId == training.Id);

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                Response.StatusCode = 400;
                return Json("Training title is required.");
            }

            if (model.PassingPercentage < 0 || model.PassingPercentage > 100)
            {
                Response.StatusCode = 400;
                return Json("Passing percentage must be between 0 and 100.");
            }

            if (!hasAttempts)
            {
                if (model.Questions == null || model.Questions.Count == 0)
                {
                    Response.StatusCode = 400;
                    return Json("At least one quiz question is required.");
                }
            }

            if (hasAttempts)
            {
                if ((training.PassingPercentage ?? 0) != model.PassingPercentage)
                {
                    Response.StatusCode = 400;
                    return Json("Passing percentage cannot be changed after users have attempted the quiz.");
                }
            }

            var pdfPath = training.PdfPath;
            var videoPath = training.VideoPath;
            if (model.PdfFile != null)
            {
                pdfPath = SaveTrainingFile(model.PdfFile, "pdf");
            }
            if (model.VideoFile != null)
            {
                videoPath = SaveTrainingFile(model.VideoFile, "video");
            }

            if (string.IsNullOrWhiteSpace(pdfPath) && string.IsNullOrWhiteSpace(videoPath))
            {
                Response.StatusCode = 400;
                return Json("Attach at least one training content file (pdf/video).");
            }

            training.Title = model.Title.Trim();
            training.Description = model.Description;
            training.PdfPath = pdfPath;
            training.VideoPath = videoPath;
            training.PassingPercentage = model.PassingPercentage;

            if (!hasAttempts)
            {
                var existingQuestions = await db.QuizQuestions
                    .Where(x => x.TrainingContentId == training.Id)
                    .ToListAsync();

                var existingQuestionIds = existingQuestions.Select(x => x.Id).ToList();
                if (existingQuestionIds.Count > 0)
                {
                    var existingOptions = await db.QuizOptions
                        .Where(x => x.QuestionId.HasValue && existingQuestionIds.Contains(x.QuestionId.Value))
                        .ToListAsync();

                    if (existingOptions.Count > 0)
                    {
                        db.QuizOptions.RemoveRange(existingOptions);
                    }

                    db.QuizQuestions.RemoveRange(existingQuestions);
                    await db.SaveChangesAsync();
                }

                foreach (var question in model.Questions.Where(q => !string.IsNullOrWhiteSpace(q.QuestionText)))
                {
                    var quizQuestion = new QuizQuestion
                    {
                        TrainingContentId = training.Id,
                        QuestionText = question.QuestionText.Trim()
                    };
                    db.QuizQuestions.Add(quizQuestion);
                    await db.SaveChangesAsync();

                    if (question.Options == null)
                    {
                        continue;
                    }

                    foreach (var option in question.Options.Where(o => !string.IsNullOrWhiteSpace(o.OptionText)))
                    {
                        db.QuizOptions.Add(new QuizOption
                        {
                            QuestionId = quizQuestion.Id,
                            OptionText = option.OptionText.Trim(),
                            IsCorrect = option.IsCorrect
                        });
                    }
                }

                await db.SaveChangesAsync();
            }

            await db.SaveChangesAsync();
            await SaveRoleAssignments(training.Id, model.RoleIds ?? new List<string>());

            return Json(new { trainingId = training.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> Create(TrainingCreateViewModel model)
        {
            if (model == null)
            {
                Response.StatusCode = 400;
                return Json("Invalid payload.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                Response.StatusCode = 400;
                return Json("Training title is required.");
            }

            if (model.PdfFile == null && model.VideoFile == null)
            {
                Response.StatusCode = 400;
                return Json("Attach at least one training content file (pdf/video).");
            }

            if (model.PassingPercentage < 0 || model.PassingPercentage > 100)
            {
                Response.StatusCode = 400;
                return Json("Passing percentage must be between 0 and 100.");
            }

            if (model.Questions == null || model.Questions.Count == 0)
            {
                Response.StatusCode = 400;
                return Json("At least one quiz question is required.");
            }

            string pdfPath = SaveTrainingFile(model.PdfFile, "pdf");
            string videoPath = SaveTrainingFile(model.VideoFile, "video");

            var training = new TrainingContent
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                PdfPath = pdfPath,
                VideoPath = videoPath,
                PassingPercentage = model.PassingPercentage
            };
            db.TrainingContents.Add(training);
            await db.SaveChangesAsync();

            foreach (var question in model.Questions.Where(q => !string.IsNullOrWhiteSpace(q.QuestionText)))
            {
                var quizQuestion = new QuizQuestion
                {
                    TrainingContentId = training.Id,
                    QuestionText = question.QuestionText.Trim()
                };
                db.QuizQuestions.Add(quizQuestion);
                await db.SaveChangesAsync();

                if (question.Options == null)
                {
                    continue;
                }

                foreach (var option in question.Options.Where(o => !string.IsNullOrWhiteSpace(o.OptionText)))
                {
                    db.QuizOptions.Add(new QuizOption
                    {
                        QuestionId = quizQuestion.Id,
                        OptionText = option.OptionText.Trim(),
                        IsCorrect = option.IsCorrect
                    });
                }
            }

            await db.SaveChangesAsync();
            await SaveRoleAssignments(training.Id, model.RoleIds ?? new List<string>());

            return Json(new { trainingId = training.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> UpdateRoles(int trainingContentId, List<string> roleIds)
        {
            var training = await db.TrainingContents.FindAsync(trainingContentId);
            if (training == null)
            {
                Response.StatusCode = 404;
                return Json("Training not found.");
            }

            await SaveRoleAssignments(trainingContentId, roleIds ?? new List<string>());
            return Json("OK");
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        public async Task<JsonResult> GetRoles()
        {
            var roles = await db.AspNetRoles
                .Select(x => new { x.Id, x.Name })
                .OrderBy(x => x.Name)
                .ToListAsync();
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        private string SaveTrainingFile(System.Web.HttpPostedFileBase file, string folder)
        {
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            string safeFileName = Path.GetFileName(file.FileName);
            string extension = Path.GetExtension(safeFileName) ?? string.Empty;
            string generatedName = string.Format("{0}{1}", Guid.NewGuid().ToString("N"), extension);
            string relativeFolder = string.Format("~/attachments/training/{0}", folder);
            string absoluteFolder = Server.MapPath(relativeFolder);

            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }

            string absolutePath = Path.Combine(absoluteFolder, generatedName);
            file.SaveAs(absolutePath);
            return string.Format("/attachments/training/{0}/{1}", folder, generatedName);
        }

        private async Task<Dictionary<int, List<string>>> GetAssignmentsByTrainingIds(List<int> trainingIds)
        {
            var results = new Dictionary<int, List<string>>();
            if (trainingIds == null || trainingIds.Count == 0)
            {
                return results;
            }

            var raw = await db.TrainingRoleAssignments
                .Where(x => trainingIds.Contains(x.TrainingContentId))
                .Select(x => new { x.TrainingContentId, x.RoleId })
                .ToListAsync();

            foreach (var row in raw)
            {
                if (!results.ContainsKey(row.TrainingContentId))
                {
                    results[row.TrainingContentId] = new List<string>();
                }
                results[row.TrainingContentId].Add(row.RoleId);
            }

            return results;
        }

        private async Task SaveRoleAssignments(int trainingContentId, List<string> roleIds)
        {
            var existing = await db.TrainingRoleAssignments
                .Where(x => x.TrainingContentId == trainingContentId)
                .ToListAsync();

            if (existing.Count > 0)
            {
                db.TrainingRoleAssignments.RemoveRange(existing);
                await db.SaveChangesAsync();
            }

            var cleanedRoleIds = roleIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string roleId in cleanedRoleIds)
            {
                db.TrainingRoleAssignments.Add(new TrainingRoleAssignment
                {
                    TrainingContentId = trainingContentId,
                    RoleId = roleId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                identityDb.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

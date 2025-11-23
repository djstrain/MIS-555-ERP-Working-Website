using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages
{
    public class CustomerRelationshipModel : PageModel
    {
        private readonly AppDbContents _db;
        private readonly ILogger<CustomerRelationshipModel> _logger;
        public CustomerRelationshipModel(AppDbContents db, ILogger<CustomerRelationshipModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Lists
        public List<Company> Companies { get; set; } = new();
        public List<Contact> Contacts { get; set; } = new();
        public List<Opportunity> Opportunities { get; set; } = new();
        public List<Activity> Activities { get; set; } = new();
        public List<Note> Notes { get; set; } = new();

    // Metrics
    public int TotalCompanies { get; set; }
    public int TotalContacts { get; set; }
    public Opportunity? TopOpportunity { get; set; }

        [BindProperty(SupportsGet = true)] public string? ActiveModule { get; set; }

        // Form bind properties
        [BindProperty] public Company NewCompany { get; set; } = new();
        [BindProperty] public Contact NewContact { get; set; } = new();
        [BindProperty] public Opportunity NewOpportunity { get; set; } = new();
        [BindProperty] public Activity NewActivity { get; set; } = new();
        [BindProperty] public Note NewNote { get; set; } = new();

    // Edit bind properties (used in modals)
    [BindProperty] public Company EditCompany { get; set; } = new();
    [BindProperty] public Contact EditContact { get; set; } = new();
    [BindProperty] public Opportunity EditOpportunity { get; set; } = new();
    [BindProperty] public Activity EditActivity { get; set; } = new();
    [BindProperty] public Note EditNote { get; set; } = new();
        // Optional typed company name for Contact creation (creates if not existing)
        [BindProperty] public string? NewContactCompanyName { get; set; }
        // Optional typed company name for Opportunity creation
        [BindProperty] public string? NewOpportunityCompanyName { get; set; }
        // Surface validation errors to UI
        public List<string> ValidationErrors { get; set; } = new();

        public async Task OnGetAsync()
        {
            ActiveModule = NormalizeModule(ActiveModule);
            // Load limited sets for performance; can expand later
            Companies = await _db.Companies.OrderByDescending(c => c.Id).Take(25).ToListAsync();
            Contacts = await _db.Contacts.OrderByDescending(c => c.Id).Take(25).Include(c => c.Company).ToListAsync();
            Opportunities = await _db.Opportunities.OrderByDescending(o => o.Id).Take(25).Include(o => o.Company).ToListAsync();
            Activities = await _db.Activities.OrderByDescending(a => a.Id).Take(20).Include(a => a.Company).Include(a => a.Contact).ToListAsync();
            Notes = await _db.Notes.OrderByDescending(n => n.Id).Take(20).Include(n => n.Company).Include(n => n.Contact).ToListAsync();

            // Compute metrics across full dataset (not just limited lists)
            TotalCompanies = await _db.Companies.CountAsync();
            TotalContacts = await _db.Contacts.CountAsync();
            TopOpportunity = await _db.Opportunities
                .OrderByDescending(o => o.Value ?? 0)
                .Include(o => o.Company)
                .FirstOrDefaultAsync();
        }

        public async Task<IActionResult> OnPostAddCompanyAsync()
        {
            ValidationErrors.Clear();
            // Restrict validation only to the Company object to avoid cross-form required errors
            ModelState.Clear();
            var isValid = TryValidateModel(NewCompany, nameof(NewCompany));
            if (string.IsNullOrWhiteSpace(NewCompany.Name))
            {
                isValid = false;
                ValidationErrors.Add("Company Name is required.");
            }
            if (!isValid)
            {
                // Collect annotation errors (length, etc.)
                ValidationErrors.AddRange(ModelState.SelectMany(k => k.Value!.Errors).Select(e => e.ErrorMessage));
                ValidationErrors = ValidationErrors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
                _logger.LogWarning("Company add failed validation: Name='{Name}' Errors={Errors}", NewCompany.Name, string.Join(";", ValidationErrors));
                ActiveModule = "companies";
                await OnGetAsync();
                return Page();
            }
            NewCompany.CreatedAt = DateTime.UtcNow;
            _db.Companies.Add(NewCompany);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added Company Id={Id} Name={Name}", NewCompany.Id, NewCompany.Name);
            return RedirectWithModule("companies");
        }

        public async Task<IActionResult> OnPostEditCompanyAsync()
        {
            ValidationErrors.Clear();
            if (EditCompany.Id <= 0)
            {
                ValidationErrors.Add("Invalid Company Id.");
            }
            if (string.IsNullOrWhiteSpace(EditCompany.Name))
            {
                ValidationErrors.Add("Company Name is required.");
            }
            if (ValidationErrors.Any())
            {
                _logger.LogWarning("Company edit failed validation Id={Id} Errors={Errors}", EditCompany.Id, string.Join(";", ValidationErrors));
                ActiveModule = "companies";
                await OnGetAsync();
                return Page();
            }
            var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Id == EditCompany.Id);
            if (existing == null)
            {
                _logger.LogWarning("Company edit failed - not found Id={Id}", EditCompany.Id);
                return RedirectWithModule("companies");
            }
            existing.Name = EditCompany.Name.Trim();
            existing.Industry = EditCompany.Industry?.Trim();
            existing.Website = EditCompany.Website?.Trim();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Edited Company Id={Id}", existing.Id);
            return RedirectWithModule("companies");
        }

        public async Task<IActionResult> OnPostDeleteCompanyAsync(int id)
        {
            var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (existing != null)
            {
                _db.Companies.Remove(existing);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted Company Id={Id}", id);
            }
            return RedirectWithModule("companies");
        }

        public async Task<IActionResult> OnPostAddContactAsync()
        {
            ValidationErrors.Clear();
            ModelState.Clear();
            var isValid = TryValidateModel(NewContact, nameof(NewContact));
            if (string.IsNullOrWhiteSpace(NewContact.FirstName)) { isValid = false; ValidationErrors.Add("First Name is required."); }
            if (string.IsNullOrWhiteSpace(NewContact.LastName)) { isValid = false; ValidationErrors.Add("Last Name is required."); }
            if (!isValid)
            {
                ValidationErrors.AddRange(ModelState.SelectMany(k => k.Value!.Errors).Select(e => e.ErrorMessage));
                ValidationErrors = ValidationErrors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
                _logger.LogWarning("Contact add failed validation: First='{First}' Last='{Last}' Errors={Errors}", NewContact.FirstName, NewContact.LastName, string.Join(";", ValidationErrors));
                ActiveModule = "contacts";
                await OnGetAsync();
                return Page();
            }
            // Handle typed company name if no CompanyId selected
            if ((NewContact.CompanyId == null || NewContact.CompanyId <= 0) && !string.IsNullOrWhiteSpace(NewContactCompanyName))
            {
                var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == NewContactCompanyName.Trim().ToLower());
                if (existing == null)
                {
                    existing = new Company { Name = NewContactCompanyName!.Trim(), CreatedAt = DateTime.UtcNow };
                    _db.Companies.Add(existing);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Created new Company from contact entry Id={Id} Name={Name}", existing.Id, existing.Name);
                }
                NewContact.CompanyId = existing.Id;
            }
            NewContact.CreatedAt = DateTime.UtcNow;
            _db.Contacts.Add(NewContact);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added Contact Id={Id} Name={First} {Last}", NewContact.Id, NewContact.FirstName, NewContact.LastName);
            return RedirectWithModule("contacts");
        }

        public async Task<IActionResult> OnPostEditContactAsync()
        {
            ValidationErrors.Clear();
            if (EditContact.Id <= 0) ValidationErrors.Add("Invalid Contact Id.");
            if (string.IsNullOrWhiteSpace(EditContact.FirstName)) ValidationErrors.Add("First Name required.");
            if (string.IsNullOrWhiteSpace(EditContact.LastName)) ValidationErrors.Add("Last Name required.");
            if (ValidationErrors.Any())
            {
                _logger.LogWarning("Contact edit failed validation Id={Id} Errors={Errors}", EditContact.Id, string.Join(";", ValidationErrors));
                ActiveModule = "contacts";
                await OnGetAsync();
                return Page();
            }
            var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == EditContact.Id);
            if (existing == null)
            {
                _logger.LogWarning("Contact edit failed - not found Id={Id}", EditContact.Id);
                return RedirectWithModule("contacts");
            }
            existing.FirstName = EditContact.FirstName.Trim();
            existing.LastName = EditContact.LastName.Trim();
            existing.Email = EditContact.Email?.Trim();
            existing.Phone = EditContact.Phone?.Trim();
            if (EditContact.CompanyId > 0) existing.CompanyId = EditContact.CompanyId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Edited Contact Id={Id}", existing.Id);
            return RedirectWithModule("contacts");
        }

        public async Task<IActionResult> OnPostDeleteContactAsync(int id)
        {
            var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (existing != null)
            {
                _db.Contacts.Remove(existing);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted Contact Id={Id}", id);
            }
            return RedirectWithModule("contacts");
        }

        public async Task<IActionResult> OnPostAddOpportunityAsync()
        {
            ValidationErrors.Clear();
            ModelState.Clear();
            var isValid = TryValidateModel(NewOpportunity, nameof(NewOpportunity));
            if (string.IsNullOrWhiteSpace(NewOpportunity.Name)) { isValid = false; ValidationErrors.Add("Opportunity Name is required."); }
            
            // Handle typed company name if no CompanyId selected
            if (NewOpportunity.CompanyId <= 0 && !string.IsNullOrWhiteSpace(NewOpportunityCompanyName))
            {
                var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == NewOpportunityCompanyName.Trim().ToLower());
                if (existing == null)
                {
                    existing = new Company { Name = NewOpportunityCompanyName!.Trim(), CreatedAt = DateTime.UtcNow };
                    _db.Companies.Add(existing);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Created new Company from opportunity entry Id={Id} Name={Name}", existing.Id, existing.Name);
                }
                NewOpportunity.CompanyId = existing.Id;
            }
            
            if (NewOpportunity.CompanyId <= 0) { isValid = false; ValidationErrors.Add("Company is required for an Opportunity (type or select)."); }
            
            if (!isValid)
            {
                ValidationErrors.AddRange(ModelState.SelectMany(k => k.Value!.Errors).Select(e => e.ErrorMessage));
                ValidationErrors = ValidationErrors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
                _logger.LogWarning("Opportunity add failed validation: CompanyId={CompanyId} Name='{Name}' Errors={Errors}", NewOpportunity.CompanyId, NewOpportunity.Name, string.Join(";", ValidationErrors));
                ActiveModule = "opportunities";
                await OnGetAsync();
                return Page();
            }
            NewOpportunity.CreatedAt = DateTime.UtcNow;
            _db.Opportunities.Add(NewOpportunity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added Opportunity Id={Id} CompanyId={CompanyId} Name={Name}", NewOpportunity.Id, NewOpportunity.CompanyId, NewOpportunity.Name);
            return RedirectWithModule("opportunities");
        }

        public async Task<IActionResult> OnPostEditOpportunityAsync()
        {
            ValidationErrors.Clear();
            if (EditOpportunity.Id <= 0) ValidationErrors.Add("Invalid Opportunity Id.");
            if (string.IsNullOrWhiteSpace(EditOpportunity.Name)) ValidationErrors.Add("Name required.");
            if (EditOpportunity.CompanyId <= 0) ValidationErrors.Add("Company required.");
            if (ValidationErrors.Any())
            {
                _logger.LogWarning("Opportunity edit failed validation Id={Id} Errors={Errors}", EditOpportunity.Id, string.Join(";", ValidationErrors));
                ActiveModule = "opportunities";
                await OnGetAsync();
                return Page();
            }
            var existing = await _db.Opportunities.FirstOrDefaultAsync(o => o.Id == EditOpportunity.Id);
            if (existing == null)
            {
                _logger.LogWarning("Opportunity edit failed - not found Id={Id}", EditOpportunity.Id);
                return RedirectWithModule("opportunities");
            }
            existing.Name = EditOpportunity.Name.Trim();
            existing.Stage = EditOpportunity.Stage?.Trim();
            existing.Value = EditOpportunity.Value;
            existing.CloseDate = EditOpportunity.CloseDate;
            existing.CompanyId = EditOpportunity.CompanyId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Edited Opportunity Id={Id}", existing.Id);
            return RedirectWithModule("opportunities");
        }

        public async Task<IActionResult> OnPostDeleteOpportunityAsync(int id)
        {
            var existing = await _db.Opportunities.FirstOrDefaultAsync(o => o.Id == id);
            if (existing != null)
            {
                _db.Opportunities.Remove(existing);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted Opportunity Id={Id}", id);
            }
            return RedirectWithModule("opportunities");
        }

        public async Task<IActionResult> OnPostAddActivityAsync()
        {
            ValidationErrors.Clear();
            ModelState.Clear();
            var isValid = TryValidateModel(NewActivity, nameof(NewActivity));
            if (!isValid)
            {
                ValidationErrors.AddRange(ModelState.SelectMany(k => k.Value!.Errors).Select(e => e.ErrorMessage));
                ValidationErrors = ValidationErrors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
                _logger.LogWarning("Activity add failed validation: Errors={Errors}", string.Join(";", ValidationErrors));
                ActiveModule = "activities";
                await OnGetAsync();
                return Page();
            }
            NewActivity.CreatedAt = DateTime.UtcNow;
            if (NewActivity.ActivityDate == default) NewActivity.ActivityDate = DateTime.UtcNow;
            _db.Activities.Add(NewActivity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added Activity Id={Id} Type={Type} Subject={Subject}", NewActivity.Id, NewActivity.ActivityType, NewActivity.Subject);
            return RedirectWithModule("activities");
        }

        public async Task<IActionResult> OnPostEditActivityAsync()
        {
            ValidationErrors.Clear();
            if (EditActivity.Id <= 0) ValidationErrors.Add("Invalid Activity Id.");
            if (ValidationErrors.Any())
            {
                _logger.LogWarning("Activity edit failed validation Id={Id} Errors={Errors}", EditActivity.Id, string.Join(";", ValidationErrors));
                ActiveModule = "activities";
                await OnGetAsync();
                return Page();
            }
            var existing = await _db.Activities.FirstOrDefaultAsync(a => a.Id == EditActivity.Id);
            if (existing == null)
            {
                _logger.LogWarning("Activity edit failed - not found Id={Id}", EditActivity.Id);
                return RedirectWithModule("activities");
            }
            existing.ActivityType = EditActivity.ActivityType?.Trim();
            existing.Subject = EditActivity.Subject?.Trim();
            existing.ActivityDate = EditActivity.ActivityDate == default ? existing.ActivityDate : EditActivity.ActivityDate;
            existing.CompanyId = EditActivity.CompanyId > 0 ? EditActivity.CompanyId : existing.CompanyId;
            existing.ContactId = EditActivity.ContactId > 0 ? EditActivity.ContactId : existing.ContactId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Edited Activity Id={Id}", existing.Id);
            return RedirectWithModule("activities");
        }

        public async Task<IActionResult> OnPostDeleteActivityAsync(int id)
        {
            var existing = await _db.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (existing != null)
            {
                _db.Activities.Remove(existing);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted Activity Id={Id}", id);
            }
            return RedirectWithModule("activities");
        }

        public async Task<IActionResult> OnPostAddNoteAsync()
        {
            ValidationErrors.Clear();
            ModelState.Clear();
            var isValid = TryValidateModel(NewNote, nameof(NewNote));
            if (string.IsNullOrWhiteSpace(NewNote.Content)) { isValid = false; ValidationErrors.Add("Note Content is required."); }
            if (!isValid)
            {
                ValidationErrors.AddRange(ModelState.SelectMany(k => k.Value!.Errors).Select(e => e.ErrorMessage));
                ValidationErrors = ValidationErrors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
                _logger.LogWarning("Note add failed validation: Errors={Errors}", string.Join(";", ValidationErrors));
                ActiveModule = "notes";
                await OnGetAsync();
                return Page();
            }
            NewNote.CreatedAt = DateTime.UtcNow;
            _db.Notes.Add(NewNote);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Added Note Id={Id} Content={Content}", NewNote.Id, NewNote.Content.Substring(0, Math.Min(50, NewNote.Content.Length)));
            return RedirectWithModule("notes");
        }

        public async Task<IActionResult> OnPostEditNoteAsync()
        {
            ValidationErrors.Clear();
            if (EditNote.Id <= 0) ValidationErrors.Add("Invalid Note Id.");
            if (string.IsNullOrWhiteSpace(EditNote.Content)) ValidationErrors.Add("Content required.");
            if (ValidationErrors.Any())
            {
                _logger.LogWarning("Note edit failed validation Id={Id} Errors={Errors}", EditNote.Id, string.Join(";", ValidationErrors));
                ActiveModule = "notes";
                await OnGetAsync();
                return Page();
            }
            var existing = await _db.Notes.FirstOrDefaultAsync(n => n.Id == EditNote.Id);
            if (existing == null)
            {
                _logger.LogWarning("Note edit failed - not found Id={Id}", EditNote.Id);
                return RedirectWithModule("notes");
            }
            existing.Content = EditNote.Content.Trim();
            if (EditNote.CompanyId > 0) existing.CompanyId = EditNote.CompanyId;
            if (EditNote.ContactId > 0) existing.ContactId = EditNote.ContactId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Edited Note Id={Id}", existing.Id);
            return RedirectWithModule("notes");
        }

        public async Task<IActionResult> OnPostDeleteNoteAsync(int id)
        {
            var existing = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (existing != null)
            {
                _db.Notes.Remove(existing);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Deleted Note Id={Id}", id);
            }
            return RedirectWithModule("notes");
        }

        private string? NormalizeModule(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim().ToLowerInvariant();
            return value switch
            {
                "companies" => "companies",
                "contacts" => "contacts",
                "opportunities" => "opportunities",
                "activities" => "activities",
                "notes" => "notes",
                _ => null
            };
        }

        private IActionResult RedirectWithModule(string module)
        {
            return RedirectToPage("/CustomerRelationship", new { ActiveModule = module });
        }
    }
}
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

        [BindProperty(SupportsGet = true)] public string? ActiveModule { get; set; }

        // Form bind properties
        [BindProperty] public Company NewCompany { get; set; } = new();
        [BindProperty] public Contact NewContact { get; set; } = new();
        [BindProperty] public Opportunity NewOpportunity { get; set; } = new();
        [BindProperty] public Activity NewActivity { get; set; } = new();
        [BindProperty] public Note NewNote { get; set; } = new();
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
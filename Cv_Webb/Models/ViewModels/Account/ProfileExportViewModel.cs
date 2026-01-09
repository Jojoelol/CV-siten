// Denna klass representerar strukturen i XML-filen
public class ProfileExportModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Description { get; set; }
    public string Skills { get; set; }
    public string Education { get; set; }
    public string Experience { get; set; }

    // Lista på projekt som användaren skapat eller är involverad i
    public List<ProjectExportModel> Projects { get; set; }
}

public class ProjectExportModel
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
}
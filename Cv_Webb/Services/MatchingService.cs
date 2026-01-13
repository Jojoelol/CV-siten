using CV_siten.Data.Models;

namespace CV_siten.Services
{
    public static class MatchingService
    {
        public static int CalculateMatchScore(Person a, Person b)
        {
            int score = 0;

            // 1. Matcha JobTitle (3 poäng)
            if (!string.IsNullOrEmpty(a.JobTitle) && a.JobTitle.Equals(b.JobTitle, StringComparison.OrdinalIgnoreCase))
                score += 3;

            // 2. Matcha Skills (2 poäng per skill)
            if (!string.IsNullOrEmpty(a.Skills) && !string.IsNullOrEmpty(b.Skills))
            {
                var skillsA = a.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());
                var skillsB = b.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToLower());

                foreach (var skA in skillsA)
                {
                    foreach (var skB in skillsB)
                    {
                        if (FuzzySkillMatch(skA, skB)) { score += 2; break; }
                    }
                }
            }

            // 3. Matcha Education (2 poäng)
            if (!string.IsNullOrEmpty(a.Education) && a.Education.Equals(b.Education, StringComparison.OrdinalIgnoreCase))
                score += 2;

            return score;
        }

        private static bool FuzzySkillMatch(string a, string b)
        {
            if (a == b || a.Contains(b) || b.Contains(a)) return true;

            // Enklare synonymlista
            if ((a == "js" && b == "javascript") || (a == "javascript" && b == "js")) return true;
            if ((a == "c#" && b.Contains("sharp")) || (a.Contains("sharp") && b == "c#")) return true;

            return false;
        }
    }
}
using MudBlazorLab.Components.Models;

namespace MudBlazorLab.Components.Services;

public static class AqlService
{
    public static int ComputeSampleSize(int lotSize, string inspectionLevel)
    {
        var lvl = (inspectionLevel ?? "II").ToUpperInvariant();
        if (lvl == "I")
        {
            if (lotSize <= 50) return 8;
            if (lotSize <= 90) return 13;
            if (lotSize <= 150) return 20;
            if (lotSize <= 280) return 32;
            if (lotSize <= 500) return 50;
            if (lotSize <= 1200) return 80;
            if (lotSize <= 3200) return 125;
            return 200;
        }
        if (lvl == "III")
        {
            if (lotSize <= 50) return 13;
            if (lotSize <= 90) return 20;
            if (lotSize <= 150) return 32;
            if (lotSize <= 280) return 50;
            if (lotSize <= 500) return 80;
            if (lotSize <= 1200) return 125;
            if (lotSize <= 3200) return 200;
            return 315;
        }
        if (lotSize <= 50) return 8;
        if (lotSize <= 90) return 13;
        if (lotSize <= 150) return 20;
        if (lotSize <= 280) return 32;
        if (lotSize <= 500) return 50;
        if (lotSize <= 1200) return 80;
        if (lotSize <= 3200) return 125;
        if (lotSize <= 10000) return 200;
        return 315;
    }

    public static (int majorAccept, int minorAccept) GetAcceptanceNumbers(int sampleSize, double aqlMajor, double aqlMinor)
    {
        if (sampleSize <= 8) return (0, 1);
        if (sampleSize <= 13) return (1, 1);
        if (sampleSize <= 20) return (1, 2);
        if (sampleSize <= 32) return (2, 3);
        if (sampleSize <= 50) return (3, 5);
        if (sampleSize <= 80) return (5, 7);
        if (sampleSize <= 125) return (7, 10);
        if (sampleSize <= 200) return (10, 14);
        if (sampleSize <= 315) return (14, 21);
        return (21, 32);
    }
}


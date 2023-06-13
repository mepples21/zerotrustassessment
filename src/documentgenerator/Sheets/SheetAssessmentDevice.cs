using Syncfusion.XlsIO;
using ZeroTrustAssessment.DocumentGenerator.Graph;

namespace ZeroTrustAssessment.DocumentGenerator.Sheets;

public class SheetAssessmentDevice : SheetBase
{
    public SheetAssessmentDevice(IWorksheet sheet, GraphData graphData) : base(sheet, graphData)
    {
    }

    public void Generate()
    {
        MdmWindowsEnrollment();
        DeviceEnrollmentConfiguration();
        DeviceComplianceConfiguration();
    }

    private void MdmWindowsEnrollment()
    {
        var isMdmEnrolled = _graphData.MobilityManagementPolicies?.Any(x => x.IsValid == true && x?.AppliesTo != PolicyScope.None);
        var result = isMdmEnrolled.HasValue && isMdmEnrolled.Value ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        SetValue("CH00001_WindowsEnrollment", result);
    }

    private void DeviceEnrollmentConfiguration()
    {
        var enrollmentConfigs = _graphData.DeviceEnrollmentConfigurations?.Where(x => x.DeviceEnrollmentConfigurationType == DeviceEnrollmentConfigurationType.SinglePlatformRestriction);
        var android = AssessmentValue.NotStarted;
        var windows = AssessmentValue.NotStarted;
        var ios = AssessmentValue.NotStarted;
        var macos = AssessmentValue.NotStarted;

        //Check if any of the platforms are disabled in the default policy
        var defaultPlatformRestrictions = _graphData.DeviceEnrollmentConfigurations?
            .Where(x => x.DeviceEnrollmentConfigurationType == DeviceEnrollmentConfigurationType.PlatformRestrictions).FirstOrDefault();
        if (defaultPlatformRestrictions != null && defaultPlatformRestrictions is DeviceEnrollmentPlatformRestrictionsConfiguration defaultRestriction)
        {
            if (defaultRestriction?.AndroidRestriction?.PlatformBlocked == false ||
                defaultRestriction?.AndroidForWorkRestriction?.PlatformBlocked == false) { android = AssessmentValue.Completed; }
            if (defaultRestriction?.IosRestriction?.PlatformBlocked == false) { ios = AssessmentValue.Completed; }
            if (defaultRestriction?.MacRestriction?.PlatformBlocked == false) { macos = AssessmentValue.Completed; }
            if (defaultRestriction?.WindowsRestriction?.PlatformBlocked == false) { windows = AssessmentValue.Completed; }
        }

        if (enrollmentConfigs != null) //Check if a custom policy has been created and mark as completed if one is found.
        {
            foreach (var config in enrollmentConfigs)
            {
                if (config is DeviceEnrollmentPlatformRestrictionConfiguration restriction)
                {
                    switch (restriction.PlatformType)
                    {
                        case EnrollmentRestrictionPlatformType.Android:
                        case EnrollmentRestrictionPlatformType.AndroidForWork:
                            android = AssessmentValue.Completed;
                            break;
                        case EnrollmentRestrictionPlatformType.Windows:
                            windows = AssessmentValue.Completed;
                            break;
                        case EnrollmentRestrictionPlatformType.Ios:
                            ios = AssessmentValue.Completed;
                            break;
                        case EnrollmentRestrictionPlatformType.Mac:
                            macos = AssessmentValue.Completed;
                            break;
                    }
                }
            }
        }

        //TODO: Future enhancement: Check transitive group count and if less than 10 (or maybe 10% of users) then set to In Progress.
        SetValue("CH00002_DeviceEnrollment_Android", android);
        SetValue("CH00003_DeviceEnrollment_Windows", windows);
        SetValue("CH00004_DeviceEnrollment_iOS", ios);
        SetValue("CH00005_DeviceEnrollment_macOS", macos);
    }

    private void DeviceComplianceConfiguration()
    {
        var compliancePolicies = _graphData.DeviceCompliancePolicies ?? new List<DeviceCompliancePolicy>();

        var aospDeviceOwnerCompliancePolicies = new List<AospDeviceOwnerCompliancePolicy>();
        var androidCompliancePolicies = new List<AndroidCompliancePolicy>(); //Device admin
        var androidDeviceOwnerCompliancePolicies = new List<AndroidDeviceOwnerCompliancePolicy>();
        var androidWorkProfileCompliancePolicies = new List<AndroidWorkProfileCompliancePolicy>();
        var iosCompliancePolicies = new List<IosCompliancePolicy>();
        var macOSCompliancePolicies = new List<MacOSCompliancePolicy>();
        var windows81CompliancePolicies = new List<Windows81CompliancePolicy>();
        var windows10CompliancePolicies = new List<Windows10CompliancePolicy>();

        foreach (var policy in compliancePolicies)
        {
            if (policy is Windows10CompliancePolicy windows10CompliancePolicy) { windows10CompliancePolicies.Add(windows10CompliancePolicy); }
            else if (policy is IosCompliancePolicy iosCompliancePolicy) { iosCompliancePolicies.Add(iosCompliancePolicy); }
            else if (policy is MacOSCompliancePolicy macOSCompliancePolicy) { macOSCompliancePolicies.Add(macOSCompliancePolicy); }
            else if (policy is AndroidWorkProfileCompliancePolicy androidWorkProfileCompliancePolicy) { androidWorkProfileCompliancePolicies.Add(androidWorkProfileCompliancePolicy); }
            else if (policy is AndroidCompliancePolicy androidCompliancePolicy) { androidCompliancePolicies.Add(androidCompliancePolicy); }
            else if (policy is AospDeviceOwnerCompliancePolicy aospDeviceOwnerCompliancePolicy) { aospDeviceOwnerCompliancePolicies.Add(aospDeviceOwnerCompliancePolicy); }
            else if (policy is AndroidDeviceOwnerCompliancePolicy androidDeviceOwnerCompliancePolicy) { androidDeviceOwnerCompliancePolicies.Add(androidDeviceOwnerCompliancePolicy); }
            else if (policy is Windows81CompliancePolicy windows81CompliancePolicy) { windows81CompliancePolicies.Add(windows81CompliancePolicy); }
        }

        var aospDeviceOwnerComplianceState = aospDeviceOwnerCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var androidComplianceState = androidCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var androidDeviceOwnerComplianceState = androidDeviceOwnerCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var androidWorkProfileComplianceState = androidWorkProfileCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var iosComplianceState = iosCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var macOSComplianceState = macOSCompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var windows81ComplianceState = windows81CompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;
        var windows10ComplianceState = windows10CompliancePolicies.Count() > 0 ? AssessmentValue.Completed : AssessmentValue.NotStarted;

        //TODO: Future enhancement: Check transitive group count and if less than 10 (or maybe 10% of users) then set to In Progress.
        SetValue("CH00006_DeviceCompliance_Android_AOSP", aospDeviceOwnerComplianceState);
        SetValue("CH00007_DeviceCompliance_Android_DeviceAdmin", androidComplianceState);
        SetValue("CH00008_DeviceCompliance_Android_EntCorp", androidDeviceOwnerComplianceState);
        SetValue("CH00009_DeviceCompliance_Android_EntPersonal", androidWorkProfileComplianceState);
        SetValue("CH00009_DeviceCompliance_iOS", iosComplianceState);
        SetValue("CH00009_DeviceCompliance_macOS", macOSComplianceState);
        SetValue("CH00009_DeviceCompliance_Windows", windows10ComplianceState);
    }
}
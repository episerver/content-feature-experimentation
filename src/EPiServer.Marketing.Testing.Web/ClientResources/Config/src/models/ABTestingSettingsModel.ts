export interface ABTestingSettingsModel {
    autoPublishWinners: Array<{ label: string; value: string }>;
    confidenceLevels: Array<{ label: string; value: string }>;
    abTestingConfigTitle: string;
    abTestingConfigDescription: string;
    testDurationLabel: string;
    participationPercentLabel: string;        
    autoPublishWinnerLabel: string;
    confidenceLevelLabel: string;        
    isEnabledLabel: string;        
    inValid: string;
    participationError: string;
    durationError: string;        
    success: string;
    saveButton: string;
    cancelButton: string;
    testDuration: number;
    participationPercent: number;
    confidenceLevel: number;
    autoPublishWinner: boolean;
    isEnabled: boolean;
}
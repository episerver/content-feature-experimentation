﻿({KpiClientScript})(function (resultValue) {
    ClientKpiConverted.id = '{KpiGuid}';
    addKpiData('{KpiGuid}', '{ABTestGuid}', '{VersionId}', resultValue);
    window.dispatchEvent(ClientKpiConverted);
});
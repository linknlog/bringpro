//service to schedule jobs
(function () {
    "use strict";
    angular.module(APPNAME)
        .factory('$jobScheduleService', JobScheduleService);
    JobScheduleService.$inject = ['$baseService', '$sabio'];

    function JobScheduleService($baseService, $sabio) {
        var aSabioServiceObject = sabio.services.jobSchedule;
        var newService = $baseService.merge(true, {}, aSabioServiceObject, $baseService);

        console.log("job schedule service", aSabioServiceObject);

        return newService;
    }
})();
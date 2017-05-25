(function () {
    "use strict";

    angular.module(APPNAME)
        .factory('$jobsService', JobsServiceFactory);

    JobsServiceFactory.$inject = ['$baseService', '$sabio'];

    function JobsServiceFactory($baseService, $sabio) {
        var aSabioServiceObject = sabio.services.jobs;

        var newService = $baseService.merge(true, {}, aSabioServiceObject, $baseService);

        console.log("jobs service", aSabioServiceObject);

        return newService;
    }
})();

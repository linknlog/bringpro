//service to get JobItems for drop down
(function () {
    "use strict";
    angular.module(APPNAME)
        .factory('$jobServiceItemOptions', jobItemOptionFactory);

    jobItemOptionFactory.$inject = ['$baseService', '$sabio'];

    function jobItemOptionFactory($baseService, $sabio) {
        var aSabioServiceObject = sabio.services.jobitemoptions;
        var newService = $baseService.merge(true, {}, aSabioServiceObject, $baseService);

        console.log("Job Item option service", aSabioServiceObject);

        return newService;
    }
})();
        (function () {
            "use strict";

            angular.module(APPNAME)
                .factory('$AdminUserService', adminUserServiceFactory);
            adminUserServiceFactory.$inject = ['$baseService', '$bringpro'];

            function adminUserServiceFactory($baseService, $bringpro) {
                var abringproServiceObject = bringpro.services.adminUsers;

                var newService = $baseService.merge(true, {}, abringproServiceObject, $baseService);

                return newService;
            }
        })();
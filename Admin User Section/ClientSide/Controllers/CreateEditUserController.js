(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('adminUserController', AdminUserController);

    AdminUserController.$inject = ['$scope', '$baseController', "$AdminUserService"];

    function AdminUserController(
        $scope
        , $baseController
        , $AdminUserService) {

        var vm = this;
        vm.userRole = null;
        vm.adminUser = null;
        vm.selectedUser = null;
        vm.payLoad = { currentPage: 1, itemsPerPage: 10 };
        vm.$AdminUserService = $AdminUserService;
        vm.$scope = $scope;
        vm.userId = $("#jobId").val();
        vm.passwordRequired = false;

        vm.receiveUsers = _receiveUsers;
        vm.onUserError = _onUserError;
        vm.receiveRoles = _receiveRoles;
        vm.submitUser = _submitUser;
        vm.submitUpdateSuccess = _submitUpdateSuccess;
        vm.submitCreateSuccess = _submitCreateSuccess;
        $baseController.merge(vm, $baseController);

        vm.notify = vm.$AdminUserService.getNotifier($scope);

        render();

        function render() {

            vm.$AdminUserService.getRoles(vm.receiveRoles, vm.onUserError);

            if (vm.userId && vm.userId.length > 0) {
                vm.$AdminUserService.getUserById(vm.userId, vm.receiveUsers, vm.onUserError);
                $('#password_Container').css('display', 'none');
                $('.create-user-heading').css('display', 'none');
                $('#choose_role').css('display', 'none');
            } else {
                $('.edit-user-heading').css('display', 'none');
                vm.passwordRequired = true;
            }
        }

        function _receiveRoles(data) {
            console.log('roles', data.items);
            vm.notify(function () {
                vm.userRole = data.items;
            });
        }

        function _receiveUsers(data) {
            vm.notify(function () {
                vm.adminUser = data.item;
                console.log('data', vm.adminUser);
            });
        }

        function _submitUser(isValid) {

            var adminUserPayload = {
                'userRoleId': vm.adminUser.role.roleId
                , 'firstName': vm.adminUser.firstName
                , 'lastName': vm.adminUser.lastName
                , 'email': vm.adminUser.email
                , 'phone': vm.adminUser.phone
                , 'userId': vm.userId
            };

            if (isValid) {
                if (vm.userId && vm.userId.length > 0) {

                    vm.$AdminUserService.update(vm.userId, adminUserPayload, vm.submitUpdateSuccess, vm.onUserError);
                } else {
                    adminUserPayload.RoleId = vm.adminUser.role.roleId;
                    adminUserPayload.password = vm.adminUser.password;
                    vm.$AdminUserService.createUser(adminUserPayload, vm.submitCreateSuccess, vm.onUserError);
                }
            } else {
                console.log('Form submitted with invalid data');
            }
        }


        function _submitUpdateSuccess() {
            console.log('update success');
            vm.$alertService.success('User Updated', 'Success!' );

        }

        function _submitCreateSuccess() {
            console.log('update success');
            vm.$alertService.success('User Created', 'Success!');

        }


        function _onUserError(jqXhr, error) {
            console.error(error);
        }


    }
})();
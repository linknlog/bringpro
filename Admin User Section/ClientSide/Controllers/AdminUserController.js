(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('adminUserController', AdminUserController);

    AdminUserController.$inject = ['$scope', '$baseController', "$AdminUserService", "$uibModal"];

    function AdminUserController(
        $scope
        , $baseController
        , $AdminUserService
        , $uibModal) {

        var vm = this;
        vm.headingInfo = "Angular 101";
        vm.adminUsers = null;
        vm.selectedUser = null;
        vm.payLoad = { currentPage: 1, itemsPerPage: 10, totalItems: 0 };
        vm.$AdminUserService = $AdminUserService;
        vm.$scope = $scope;
        vm.$uibModal = $uibModal;
        vm.currentId = null;
        vm.getsResultsPage = 1;


        vm.receiveUsers = _receiveUsers;
        vm.selectUser = _selectUser;
        vm.onUserError = _onUserError;
        vm.deleteUserSuccess = _deleteUserSuccess;
        vm.openModal = _openModal;
        vm.pageChanged = _pageChanged;
        vm.searchUsers = _searchUsers;

        $baseController.merge(vm, $baseController);

        vm.notify = vm.$AdminUserService.getNotifier($scope);

        render();

        //------------------------------------------------------//
        
        function render() {
            vm.$AdminUserService.getUsers(vm.payLoad, vm.receiveUsers, vm.onUserError);
            console.log('payload', vm.payLoad);
        }
        
        //------------------------------------------------------//

        function _receiveUsers(data) {
            //this receives the data and calls the special
            //notify method that will trigger ng to refresh UI
            vm.notify(function () {
                vm.adminUsers = data.items;
                vm.payLoad.totalItems = data.totalItems;

                console.log(data.items);

                var firstNumber = ((vm.payLoad.currentPage - 1) * vm.payLoad.itemsPerPage) + 1;

                if (vm.payLoad.currentPage * 10 < vm.payLoad.totalItems) {
                    var secondNumber = (vm.payLoad.currentPage * vm.payLoad.itemsPerPage);
                    $('.infos').text('Showing ' + firstNumber + ' ' + 'to ' + secondNumber + ' ' + 'of ' + vm.payLoad.totalItems);
                } else {
                    var secondNumber = vm.payLoad.totalItems;
                    $('.infos').text('Showing ' + firstNumber + ' ' + 'to ' + secondNumber);
                }

                window.scrollTo(0, 0);

            });
        }

        //------------------------------------------------------//

        function _searchUsers() {
            vm.payLoad.Query = $('#search_box').val();
            render();
        }

        //------------------------------------------------------//

        function _pageChanged(newPage) {
            vm.payLoad.currentPage = newPage;
            render();
        };

        //------------------------------------------------------//

        function _deleteUserSuccess() {
            vm.$alertService.error('User Deleted', 'Success!');
            console.log('delete button firing');

        }
     
        //------------------------------------------------------//

        function _selectUser(anEmp) {
            console.log(anEmp);
            vm.selectedUser = anEmp;
        }

        //------------------------------------------------------//

        function _onUserError(jqXhr, error) {
            console.error(error);
            console.log('jhjkhkjhl');

        }

        //------------------------------------------------------//

        function _openModal(id) {

            vm.currentId = id;

            console.log('open modal', vm.currentId);
            var modalInstance = vm.$uibModal.open({
                animation: true,
                templateUrl: 'modalContent.html',       //  this tells it what html template to use. it must exist in a script tag OR external file
                controller: 'modalController as mc',    //  this controller must exist and be registered with angular for this to work
                size: 'sm',
                resolve: {  //  anything passed to resolve can be injected into the modal controller as shown below
                }
            });

            //  when the modal closes it returns a promise
            modalInstance.result.then(function () {
                console.log('blue button clicked');
                vm.$AdminUserService.delete(vm.currentId, vm.deleteUserSuccess, vm.onUserError);
                setTimeout(location.reload.bind(location), 800);    //  if the user closed the modal by clicking Save
            }, function () {
                //  if the user closed the modal by clicking cancel
            });
        }
    }

})();
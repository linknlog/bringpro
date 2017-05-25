        (function () {
            "use strict";

            angular.module(APPNAME)
                .controller('modalController', ModalController);

            ModalController.$inject = ['$scope', '$baseController', '$uibModalInstance']

            function ModalController(
                $scope
                , $baseController
                , $uibModalInstance
                ) {

                var vm = this;

                $baseController.merge(vm, $baseController);

                vm.$scope = $scope;
                vm.$uibModalInstance = $uibModalInstance;

                vm.ok = function () {
                    vm.$uibModalInstance.close();
                };

                vm.cancel = function () {
                    vm.$uibModalInstance.dismiss('cancel');
                };
            }
        })();



    </script>

    <script type="text/ng-template" id="modalContent.html">
        <div class="modal-header">
            <h3 class="modal-title">Are You Sure?</h3>
        </div>
        <div class="modal-body">

            <p>This operation cannot be undone!</p>
        </div>
        <div class="modal-footer">
            <button class="btn btn-danger" ng-click="mc.cancel()">Cancel</button>
            <button class="btn btn-primary" ng-click="mc.ok()">OK</button>
        </div>
    </script>
}
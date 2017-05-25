
//This is the jobFlow controller. I built this but 4 people ended up adding their needs to it too
//Tried to trim as much of their work as possible.

(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('jobController', JobController).filter('phoneNumber', filter);;


    JobController.$inject = [
        '$scope', '$baseController', "$jobsService", "$jobServiceItemOptions", "$uibModal",
        "localStorageService", "$filter", "$jobScheduleService", "$log", "$location", "$billingService",
        "$dashboardService", "$window", "$utilityService", "$userCreditsService"
    ];

    function JobController(
        $scope,
        $baseController,
        $jobsService,
        $jobServiceItemOptions,
        $uibModal,
        $localStorageService,
        $filter,
        $jobScheduleService,
        $log,
        $location,
        $billingService,
        $dashboardService,
        $window,
        $utilityService,
        $userCreditsService) {

        var vm = this;
        vm.$jobsService = $jobsService;
        vm.$jobServiceItemOptions = $jobServiceItemOptions;
        vm.$billingService = $billingService;
        vm.$dashboardService = $dashboardService;
        vm.$scope = $scope;
        vm.$uibModal = $uibModal;
        vm.$location = $location;
        vm.$window = $window;
        vm.$localStorageService = $localStorageService;
        vm.$jobScheduleService = $jobScheduleService;
        vm.$filter = $filter;
        vm.$utilityService = $utilityService;
        vm.$userCreditsService = $userCreditsService;
        vm.activeInfo = null;
        vm.jobId = null;
        vm.jobItems = null;
        vm.userCreditOptions = {};
        
        //values for the different buttons when selecting job type.
        vm.asapJobButtons = [
            { name: 'Store Pickup', value: 0, id: 0 },
            { name: 'Home Pickup', value: 1, id: 1 },
            { name: 'Labor Only', value: 2, id: 2 }
        ];

        vm.scheJobButtons = [
            { name: 'Store Pickup', value: 3, id: 3 },
            { name: 'Home Pickup', value: 4, id: 4 },
            { name: 'Labor Only', value: 5, id: 5 }
        ];

        vm.jobTimeButtons = [
            { name: '9AM to 11AM', value: 0, id: 6 },
            { name: '11AM to 1PM', value: 1, id: 7 },
            { name: '1PM to 3PM', value: 2, id: 8 },
            { name: '3PM to 5PM', value: 3, id: 9 }
        ];

        vm.wayPointServices = [
            { name: 'Pick Up', value: 0, class: 'wayPointPickup' },
            { name: 'Drop Off', value: 1, class: 'wayPointDropOff' },
            { name: 'Assembly/Installation', value: 2, class: 'wayPointAssemble' },
            { name: 'Removal of Trash', value: 3, class: 'wayPointTrash' },
            { name: 'Other', value: 4, class: 'wayPointOther' }
        ];


        vm.itemsPickup = [];
        vm.itemsDeliver = [];

        //default states for accordion panels.
        vm.accordionPanels = {
            isCustomHeaderOpen: false,
            isFirstOpen: true,
            open: true
        };

        vm.addressPanel = {
            open: true
        }

        vm.timeAccordionPanels = {
            open: true,
            isOpen: false
        }
        vm.wayPointAccordionPanels = {
            open: true,
            isOpen: false
        }

        vm.paymentPanels = {
            open: true,
            isOpen: false
        }

        vm.guestPaymentPanels = {
            open: true,
            isOpen: false
        }

        vm.jobOverviewPanels = {
            open: true,
            isOpen: false
        }

        vm.oneAtATime = true;



        vm.otherOptions = false;
        vm.deliveryOptionText = 'Choose Delivery Option';
        vm.schedulingOptionText = 'Scheduled: Choose Date & Time Slot';
        vm.asapOptionText = 'ASAP: Choose Time Slot';
        vm.updatedSchedulingOptionText = null;
        vm.updatedAsapOptionText = null;
        vm.defaultDeliveryOption = false;
        vm.updatedDeliveryOption = false;
        vm.updatedDeliveryOptionText = null;
        vm.timeSlotOptionText = null;
        vm.todayAvailability = {};

        //Edmund Need to change to false later once the Job flow is Set

        vm.activeJob = null;
        vm.activeTime = null;
        vm.currentDate = null;

        vm.wayPoints = [];

        //hardcoded quantity until server side was done.
        //our time ran out before this could be completed.
        vm.itemQuantity = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];


        vm.currentJobScheduleId = 0;
        vm.asapJobLabel = null;
        vm.scheJobLabel = null;
        vm.showDateTimePicker = false;
        vm.hideAsapButton = false;
        vm.showScheduledSubmitButton = false;
        vm.hideScheduleButton = false;
        vm.timeScheduleSubmit = true;
        vm.showScheduleTimeSelect = true;
        vm.dateToday = _convertDateToZeroTime(new Date());
        vm.availableTimeSlotsByToday = {};
        vm.jobSchedule = {};
        vm.timeSlotInfo = {
            teamId: null
        };
        vm.availableTimeSlotsByDate = {};

        vm.datePicker = false;

        vm.dateFormat = {
            formatYear: 'yy',
            minDate: _addDays(new Date(), 1),
            showWeeks: false

        };

        vm.submitText = 'Save & Add Pickup Location';

        vm.showPickupItems = false;
        vm.showDropOffItems = false;
        vm.hideTimeScheduler = true;

        vm.addressStreetNumber = null;
        vm.addressStreetName = null;
        vm.addressStreet = null;
        vm.addressCity = null;
        vm.addressState = null;
        vm.addressZipcode = null;
        vm.addressLat = null;
        vm.addressLong = null;
        vm.showTimeSlotPicker = true;
        vm.asapUnavailable = true; //For ng-hide/show on ASAP job text
        vm.paymentInfo = {};
        vm.submitPayment = _submitPayment;
        vm.customerCards = {};
        vm.getCurrentUserCCInfo = _getCurrentUserCCInfo;
        vm.openModal = _openModal;
        vm.updateSuccess = _updateSucess;
        vm.guestPaymentNonce = null;
        //website ID from the websiteViewModel - now the website ID does NOT have to be hardcoded.
        vm.websiteId = $("#websiteId").val(); 

        vm.mediaId = null;
        vm.currentItem = null;

        vm.currentPickUpItem = null;
        vm.currentDropOffItem = null;
        vm.pickupItemId = null;
        vm.dropOffItemId = null;

        //dropzone section for customers to upload images
        vm.dzOptions = {
            url: "/api/media/insert",
            clickable: ".fileinput-button",
            addRemoveLinks: true,
            acceptedFiles: ".png,.jpg,.gif,.jpeg",
            autoProcessQueue: true,
            parallelUploads: 1,
            init: function () {
                var upload = this;

                upload.on("sending",
                    function (file, xhr, formData) {
                        formData.append("mediaTypeId", 2);
                        console.log("HERE ", formData, $location);
                    });


                upload.on("success",
                    function (data, json) {
                        console.log('image data', data);
                        console.log('json data', json.item);

                        vm.mediaId = json.item;
                        vm.currentItem.mediaId = vm.mediaId;

                    })
                upload.on("error",
                    function (file, errorMessage, xhr) {
                        console.log(errorMessage);
                    });
            }

        }

        vm.dzCallbacks = {
            'addedfile': function (file) {
                console.log(file);
                $scope.newFile = file;
            },
            'success': function (file, xhr) {
                console.log(file, xhr);
            }
        }

        vm.submitCoupon = _submitCoupon;
        vm.asapJob = _asapJob;
        vm.scheJob = _scheJob;
        vm.jobTimeButton = _jobTimeButton;
        vm.addAddress = _addAddress;
        vm.openDatePicker = _openDatePicker;
        vm.submitJob = _submitJob;
        vm.addItem = _addItem;
        vm.togglePickupItems = _togglePickupItems;
        vm.toggleDropoffItems = _toggleDropoffItems;
        vm.onDateChange = _onDateChange;
        vm.deletePickupItem = _deletePickupItem;
        vm.deleteSuccess = _deleteSuccess;
        vm.wayPointSuccess = _wayPointSuccess;
        vm.jobDropZone = _jobDropZone;
        vm.submitTimeSelection = _submitTimeSelection;
        vm.setCurrentItem = _setCurrentItem;
        vm.getJobByIdSuccess = _getJobByIdSuccess;
        vm.clientToken = $("#clientCCToken").val();
        vm.insertOrUpdate = _insertOrUpdate;
        vm.submitPayment = _submitPayment;
        vm.submitCard = _submitCard;
        vm.insertBringgJobSuccess = _insertBringgJobSuccess;
        vm.insertBringgJobFail = _insertBringgJobFail;
        vm.getJob = _getJob;

        $baseController.merge(vm, $baseController);

        vm.notify = vm.$jobsService.getNotifier($scope);

        render();

        //--------------------------------------------------------//

        function render() {
            vm.$jobServiceItemOptions.getAll(vm.getJobItemsSuccess, vm.commonErrorHandler);
            _getCustomerByUserId();
            vm.getCurrentUserCCInfo();

            braintree.client.create({
                authorization: vm.clientToken
            }, _clientInstance);

            _insertOrUpdate();
        }

        //--------------------------------------------------------//

        //updates job or creates a new one.
        function _insertOrUpdate() {
            if (vm.$routeParams.jobId) {
                vm.jobId = vm.$routeParams.jobId;
                _getJob(vm.jobId);
                vm.accordionPanels.isFirstOpen = !vm.accordionPanels.isFirstOpen;
            } else {
                _addAddress();
            }
        }

        //--------------------------------------------------------//

        function _getJob(jobId) {
            vm.$jobsService.getById(jobId, _getJobByIdSuccess, _commonErrorHandler);
        }

        //--------------------------------------------------------//

        function _openDatePicker() {
            vm.datePicker = true;
        }

        //--------------------------------------------------------//

        function _asapJob() {
            vm.$alertService.success("ASAP Job Selected!");
            vm.hideScheduleButton = false;
            vm.hideAsapButton = true;
            vm.showAsapSelectedButton = true;

            //Grab ASAP Time Slot Id for Schedule Table
            vm.activeTime = vm.todayAvailability[0].id;
            console.log("Current Time Slot ASAP: ", vm.activeTime);

            //Hide Schdule stuff
            vm.showScheduledSelectedButton = false;
            vm.hideScheduleButton = false;
            vm.showScheduledSubmitButton = false;
            vm.showDateTimePicker = false;

            //Inserting Schedule After ASAP Submit
            vm.jobScheduleDate = vm.dateToday;
            _InsertOrUpdateScheduleSlot();
        }

        //--------------------------------------------------------//

        function _scheJob() {
            //Used for the ASAP time slot only
            vm.showScheduledSubmitButton = true;
            vm.hideScheduleButton = true;
            vm.hideAsapButton = false;
            vm.activeTime = null;
            vm.showDateTimePicker = true;

            //Hide ASAP Stuff
            vm.showAsapSelectedButton = false;
            vm.hideAsapButton = false;

        }

        //--------------------------------------------------------//

        function _InsertOrUpdateScheduleSlot() {
            //jobid, scheduleid, date, jobScheduleId?
            //Waypoint Stuff First
            for (var i = 0; i < vm.wayPoints.length; i++) {

                var wayPoint = vm.wayPoints[i];

                var addressComponents = wayPoint.address.address_components;

                var itemsPickUp = wayPoint.jobWaypointItemsPickup;
                var itemsDropOff = wayPoint.jobWaypointItemsDropOff;

                var wayPointItems = wayPoint.items;

                wayPoint.address.externalPlaceId = wayPoint.address.id;
                wayPoint.jobId = vm.jobId;

                console.log('this is addressComponents ', addressComponents);
                for (var j = 0; j < addressComponents.length; j++) {
                    var types = addressComponents[j]
                    var componentTypes = types.types

                    if (componentTypes) {
                        if (componentTypes == "street_number") {
                            var addressNumber = types.long_name;
                        }
                        if (componentTypes == "route") {
                            var addressName = types.long_name;

                            wayPoint.address.line1 = addressNumber + " " + addressName;
                        }
                        if (componentTypes[0] == "locality") {
                            wayPoint.address.city = types.long_name;
                        }
                        if (componentTypes[0] == "administrative_area_level_1") {
                            wayPoint.address.state = types.long_name;
                        }
                        if (componentTypes == "postal_code") {
                            wayPoint.address.zipCode = types.short_name;
                        }

                    }
                }

                for (var k = 0; k < itemsPickUp.length; k++) {
                    var itemsPickUpArray = itemsPickUp[k];

                    itemsPickUpArray.operation = 1;

                }
                for (var j = 0; j < itemsDropOff.length; j++) {
                    var itemsDropOffArray = itemsDropOff[j];

                    itemsDropOffArray.operation = 2;


                }
            }

            //Add waypoints AND add the Time Slot Info.
            var payload = {
                waypoints: vm.wayPoints
                , JobScheduleId: vm.currentJobScheduleId
                , JobId: vm.$routeParams.jobId
                , ScheduleId: vm.activeTime
                , Date: vm.jobScheduleDate
            }
            console.log("Job Schedule Payload: ", payload);
            vm.$jobsService.insertJobWaypoint(payload, _InsertOrUpdateScheduleSlotSuccess, _InsertOrUpdateScheduleSlotError);

        }

        //--------------------------------------------------------//

        function _InsertOrUpdateScheduleSlotSuccess(data) {
            vm.notify(function () {
                vm.currentJobScheduleId = data.jobScheduleId;
                console.log("Current Job Schedule Id Result: ", vm.currentJobScheduleId);

            });
        }

        //--------------------------------------------------------//

        function _InsertOrUpdateScheduleSlotError(jqXhr, error) {
            console.error(error);
        }
   
        //--------------------------------------------------------//

        function _submitJobSuccess(data) {
            console.log('data', data.item);
            vm.jobId = data.item;
            vm.$localStorageService.set('jobId', vm.jobId);
            console.log('this is the vm.jobId', vm.jobId);
            console.log('this is location ', vm.$location);
            vm.$location.url(vm.jobId);
        }

        //--------------------------------------------------------//

        function _commonErrorHandler(data) {
            console.log('data', data);
        }

        //--------------------------------------------------------//

        function _togglePickupItems() {
            vm.showPickupItems = !vm.showPickupItems;
        }

        //--------------------------------------------------------//


        function _toggleDropoffItems() {
            vm.showDropOffItems = !vm.showDropOffItems;
        }

        //--------------------------------------------------------//

        function _addAddress() {
            if (!vm.wayPoints) {
                vm.wayPoints = [];
            }
            vm.wayPoints.push({ jobWaypointItemsPickup: [{}], jobWaypointItemsDropOff: [{}] });
            vm.accordionPanels.open = false;
        }

        //--------------------------------------------------------//

        function _setCurrentItem(currentItem) {
            vm.currentItem = currentItem;

            console.log("set current item", vm.currentItem)

            //call this function using ng-click
            //dropzone should open
            //currentItem needs to be attached to vm.
            //on success take mediaId and attach it to this vm.

        }

        //--------------------------------------------------------//

        function _submitJob() {

            for (var i = 0; i < vm.wayPoints.length; i++) {

                var wayPoint = vm.wayPoints[i];

                var addressComponents = wayPoint.address.address_components;

                var itemsPickUp = wayPoint.jobWaypointItemsPickup;
                var itemsDropOff = wayPoint.jobWaypointItemsDropOff;

                var wayPointItems = wayPoint.items;

                wayPoint.address.externalPlaceId = wayPoint.address.id;
                wayPoint.jobId = vm.jobId;

                console.log('this is addressComponents ', addressComponents);
                for (var j = 0; j < addressComponents.length; j++) {
                    var types = addressComponents[j]
                    var componentTypes = types.types

                    if (componentTypes) {
                        if (componentTypes == "street_number") {
                            var addressNumber = types.long_name;
                        }
                        if (componentTypes == "route") {
                            var addressName = types.long_name;

                            wayPoint.address.line1 = addressNumber + " " + addressName;
                        }
                        if (componentTypes[0] == "locality") {
                            wayPoint.address.city = types.long_name;
                        }
                        if (componentTypes[0] == "administrative_area_level_1") {
                            wayPoint.address.state = types.long_name;
                        }
                        if (componentTypes == "postal_code") {
                            wayPoint.address.zipCode = types.short_name;
                        }

                    }
                }

                for (var k = 0; k < itemsPickUp.length; k++) {
                    var itemsPickUpArray = itemsPickUp[k];

                    itemsPickUpArray.operation = 1;

                }
                for (var j = 0; j < itemsDropOff.length; j++) {
                    var itemsDropOffArray = itemsDropOff[j];

                    itemsDropOffArray.operation = 2;


                }
            }

            var jobPayload = {
                waypoints: vm.wayPoints,
                externalCustomerId : vm.externalCustomerId
            }


            vm.$jobsService.insertJobWaypoint(jobPayload, vm.wayPointSuccess, vm.commonErrorHandler);

            console.log('insertJobWaypoint payload', jobPayload);
        }

        //--------------------------------------------------------//

        function _wayPointSuccess(data) {
            console.log('wp success data: ', data);

            _insertOrUpdate();
            console.log('waypoint successfully sent to server', data);
        }

        //--------------------------------------------------------//

        //THIS SECTION IS FOR GETTING JOBBYID AND SETTING THE ID TO EACH ITEMTYPE - DF
        function _getJobByIdSuccess(data) {
            console.log('getJobByIdSuccess', data);

            if (data.item) {
                vm.notify(function () {
                    vm.wayPoints = data.item.jobWaypoints;
                    console.log('this is vm.wayPoints', vm.wayPoints);

                    for (var i = 0; i < vm.wayPoints.length; i++) {
                        
                        for (var j = 0; j < vm.wayPoints[i].jobWaypointItemsPickup.length; j++) {
                            if (vm.wayPoints[i].jobWaypointItemsPickup[j]) {
                                if (vm.wayPoints[i].jobWaypointItemsPickup[j].operation == 1 /*&& vm.wayPoints[i].jobWaypointItemsPickup[j].id != vm.wayPoints[i].jobWaypointItemsDropOff[k].parentItemId */) {
                                    vm.itemsPickup.push(vm.wayPoints[i].jobWaypointItemsPickup[j]);
                                    console.log('vm.itemsPickup array: ', vm.itemsPickup);
                                }
                            }
                        }

                    }
                    vm.timeSlotInfo.teamId = data.item.teamId;
                    console.log("Team Id Is: ", vm.timeSlotInfo.teamId);
                });
            }

            //we loop through every wayPoint and format the address via a utility function so it is exactly like geocomplete's address
            if (vm.wayPoints) {
                for (var i = 0; i < vm.wayPoints.length; i++) {
                    // console.log('vm.wayPoints[i].address before going into utility ', vm.wayPoints[i].address);
                    if (vm.wayPoints[i].address) {
                        vm.wayPoints[i].address = vm.$utilityService.formatAddress(vm.wayPoints[i].address);
                    }
                }
                _getTimeSlotsForTeamId();
            } else { // if there are no wayPoints, we want to call _addAddress() which will create an initial accordion
                _addAddress();
            }
        }

        //--------------------------------------------------------//

        function _jobDropZone() {
            new Dropzone('.job-dropzone', { url: "/api/media/insert" });
            console.log('dropzonefiring');
        }

        //--------------------------------------------------------//

        function _addItem(pickupItemsInSpecificWaypoint) {
            pickupItemsInSpecificWaypoint.push({});
            console.log('item added...this is items pickup for this waypoint: ', pickupItemsInSpecificWaypoint);
        }

        //--------------------------------------------------------//

        function _deletePickupItem(pickUpItemsInSpecificWaypoint, index) {
            console.log("list of items in one waypoint", pickUpItemsInSpecificWaypoint);
            console.log("index of item in pickupItem list", index);

            if (pickUpItemsInSpecificWaypoint[index].id) {
                console.log('this item has an id:', pickUpItemsInSpecificWaypoint[index].id);
                _deleteItem(pickUpItemsInSpecificWaypoint[index].id);
            }
            pickUpItemsInSpecificWaypoint.splice(index, 1);
        }

        //--------------------------------------------------------//

        function _deleteItem(id) {
            vm.$jobsService.deleteJobItems(id, _onDeleteItemSuccess, _onDeleteItemError);
        }

        //--------------------------------------------------------//

        function _onDeleteItemSuccess() {
            console.log("Delete Item Success");
        }

        //--------------------------------------------------------//

        function _onDeleteItemError() {
            console.log("Delte Item Error");
        }

        //--------------------------------------------------------//

        function _deleteSuccess(data) {
            console.log('delete error', data);
        }

        //--------------------------------------------------------//

        function _getJobItemsSuccess(data) {
            vm.jobItems = data.items;
            //console.log('jobitems', vm.jobItems); 
        }

})();


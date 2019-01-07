(function () {
    'use strict';

    angular.module('ascApp').controller('DeploymentCtrl', DeploymentCtrl);

    DeploymentCtrl.$inject = ['$stateParams', 'initialData', 'ascApi'];

    /* @ngInject */
    function DeploymentCtrl($stateParams, initialData, ascApi) {
        /* jshint validthis: true */
        var vm = this;
        console.log('**deploy initData', initialData);

        vm.auditLogs = initialData.value;
        vm.correlationId = $stateParams.correlationId;
        vm.formatTime = formatTime;
        vm.refresh = refresh;
        vm.selectAudit = selectAudit;

        activate();

        ////////////////

        function activate() { }

        function formatTime(timestamp) {
            return moment(timestamp).from(moment());
        }

        function refresh() {
            ascApi.getAuditLogs($stateParams.subscriptionId, $stateParams.correlationId).then(function (data) {
                vm.auditLogs = data.value;
            });
        }
        function selectAudit(logEntry) {
            //console.log('**logEntry', logEntry);
            vm.selectedLog = logEntry;
            vm.selectedEventId = logEntry.eventDataId;

            var props = [];
            addItem(logEntry.operationName.localizedValue, 'Operation Name');
            addItem(logEntry.status.localizedValue, 'Status');
            addItem(logEntry.eventTimestamp, 'Event Timestamp');
            addItem(logEntry.operationId, 'Operation ID');
            addItem(logEntry.authorization, 'Authorization');
            addItem(logEntry.resourceUri, 'Resource URI');
            addItem(logEntry.subStatus.localizedValue, 'Sub-status');
            addItem(logEntry.resourceProviderName.localizedValue, 'Resource Provider');

            if (logEntry.properties && logEntry.properties.statusCode) {
                vm.selectedStatusCode = logEntry.properties.statusCode;
            } else {
                vm.selectedStatusCode = null;
            }

            if (logEntry.properties && logEntry.properties.statusMessage) {
                var msg = JSON.parse(logEntry.properties.statusMessage);
                vm.selectedStatusMsg = msg;
            } else {
                vm.selectedStatusMsg = null;
            }

            function addItem(item, label){
                if (item) {
                    props.push({label: label, text: item });
                }
            }

            vm.selectedLogProperties = props;
        }
    }
})();
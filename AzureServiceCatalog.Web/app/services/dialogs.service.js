(function () {
    'use strict';

    angular.module('ascApp').factory('dialogsService', dialogsService);

    dialogsService.$inject = ['$uibModal'];

    function dialogsService($uibModal) {
        var service = {
            alert: alert,
            confirm: confirm,
            confirmGroup: confirmGroup,
            openEditFieldModal: openEditFieldModal,
            openJsonModal: openJsonModal,
            errorDialog: errorDialog
        };

        return service;

        function alert(message, title) {
            var config = {
                message: message,
                title: title,
                buttons: ['OK'],
                cancelVisible: false
            };
            return openModal(config);
        }

        function confirm(message, title, buttons) {
            var config = {
                message: message,
                title: title,
                buttons: buttons,
                cancelVisible: true
            };
            return openModalLarge(config);
        }

        function confirmGroup(title, buttons) {
            var config = {
                html: true,
                title: title,
                buttons: buttons,
                cancelVisible: true
            };
            return openModalLarge(config);
        }

        function errorDialog(message, data, button) {
            var config = {
                message: message,
                requestId: data.requestId,
                requestDate: data.requestDate,
                button: button
            };
            return openErrorDialog(config);
        }

        //#region Private Methods

        function openModal(config) {
            var modalInstance = $uibModal.open({
                templateUrl: '/app/shared/confirm-modal.html',
                controller: 'ConfirmModalCtrl',
                controllerAs: 'vm',
                resolve: {
                    data: function () {
                        return config;
                    }
                },
                size: 'sm'
            });

            return modalInstance.result;
        }

        function openModalLarge(config) {
            var modalInstance = $uibModal.open({
                templateUrl: '/app/shared/confirm-modal.html',
                controller: 'ConfirmModalCtrl',
                controllerAs: 'vm',
                resolve: {
                    data: function () {
                        return config;
                    }
                },
                size: 'lg'
            });

            return modalInstance.result;
        }

        function openEditFieldModal(vm, fieldType, fieldName, blobStorageLocation) {
            var editFieldModal = $uibModal.open({
                templateUrl: '/app/shared/edit-field-modal.html',
                controller: 'EditFieldModalCtrl',
                controllerAs: 'vm',
                resolve: {
                    data: {
                        fieldValue: vm[fieldName],
                        fieldType: fieldType,
                        fieldName: fieldName,
                        blobStorageLocation: blobStorageLocation
                    }
                },
                size: 'lg'
            });
            return editFieldModal.result.then(function (result) {
                vm[fieldName] = result.fieldValue;
            });
        }

        function openJsonModal(json, title) {
            var editFieldModal = $uibModal.open({
                templateUrl: '/app/shared/json-modal.html',
                controller: 'JsonModalCtrl',
                controllerAs: 'vm',
                resolve: {
                    data: {
                        json: json,
                        title: title
                    }
                },
                size: 'lg'
            });
            return editFieldModal.result;
        }

        function openErrorDialog(config) {
            console.log(config);
            var modalInstance = $uibModal.open({
                templateUrl: '/app/shared/error-dialog-box.html',
                controller: 'ErrDialogBoxCtrl',
                controllerAs: 'vm',
                resolve: {
                    data: function () {
                        return config;
                    }
                },
                size: 'md'
            });

            return modalInstance.result;
        }


        //#endregion
    }
})();
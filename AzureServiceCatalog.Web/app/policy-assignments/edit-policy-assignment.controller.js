(function () {
    'use strict';

    angular.module('ascApp').controller('EditPolicyAssignmentCtrl', EditPolicyAssignmentCtrl);
    EditPolicyAssignmentCtrl.$inject =
        ['$q', '$state', 'initialData', 'ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function EditPolicyAssignmentCtrl($q, $state, initialData, ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;
        vm.assignments = {};
        vm.origAssignments = {};
        vm.policies = initialData.policies;
        vm.policyAssignments = initialData.policyAssignments;
        vm.saveAll = saveAll;
        vm.subscription = initialData.subscription;

        activate();


        function activate() {
            populateAssignmentsLookup();
        }

        function getPolicyAssignmentName(policy) {
            return _.kebabCase(vm.subscription.displayName + '-' + policy.name);
        }

        function populateAssignmentsLookup() {
            _.forEach(vm.policies, function (policyItem) {
                var policy = policyItem.policy;
                var isAssigned = _.find(vm.policyAssignments, function (assignment) {
                    return assignment.properties.policyDefinitionId === policy.id;
                });
                vm.assignments[policy.id] = isAssigned;
            });
            vm.origAssignments = angular.copy(vm.assignments);
        }

        function deletePolicyAssignment(policy) {
            var policyAssignmentName = getPolicyAssignmentName(policy);
            return ascApi.deletePolicyAssignment(vm.subscription.rowKey, policyAssignmentName);
        }

        function saveAll() {
            dialogs.confirm('Are you sure you want to save all Policy Assignment changes?', 'Save?', ['Yes', 'No']).then(function () {
                var promises = [];
                _.forEach(vm.policies, function (policyItem) {
                    var policy = policyItem.policy;
                    var isInOrig = vm.origAssignments[policy.id];
                    var isInCurrent = vm.assignments[policy.id];
                    if (!isInOrig && isInCurrent) {
                        promises.push(savePolicyAssignment(policy));
                    }
                    if (isInOrig && !isInCurrent) {
                        promises.push(deletePolicyAssignment(policy));
                    }
                });

                $q.all(promises).then(function (results) {
                    toastr.success('All Policy Assignments successfully saved.', 'Success');
                    $state.go('manage-policy-assignments');
                });
            });
        }

        function savePolicyAssignment(policy) {
            var policyAssignment = {
                name: getPolicyAssignmentName(policy),
                properties: {
                    policyDefinitionId: policy.id,
                    scope: '/subscriptions/' + vm.subscription.rowKey
                }
            };

            return ascApi.savePolicyAssignment(vm.subscription.rowKey, policyAssignment.name, policyAssignment);
        }
    }
})();
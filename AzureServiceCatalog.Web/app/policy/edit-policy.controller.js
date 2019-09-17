(function () {
    'use strict';

    angular.module('ascApp').controller('EditPolicyCtrl', EditPolicyCtrl);
    EditPolicyCtrl.$inject = ['$stateParams', '$timeout', '$state', 'initialData', 'ascApi', 'toastr', 'dialogsService'];

    /* @ngInject */
    function EditPolicyCtrl($stateParams, $timeout, $state, initialData, ascApi, toastr, dialogs) {
        /* jshint validthis: true */
        var vm = this;

        vm.buttonVisible = buttonVisible;
        vm.deletePolicy = deletePolicy;
        vm.lookupPathChecked = lookupPathChecked;
        vm.lookupPath = null;
        vm.newConditionSubItem = newConditionSubItem;
        vm.newOperatorSubItem = newOperatorSubItem;
        vm.policy = getEmptyPolicy();
        vm.policyItem = { if: {} };
        vm.preview = preview;
        vm.root = { id: 1, title: 'if', nodes: [] };
        vm.save = save;
        vm.toPolicy = toPolicy;
        vm.subscriptionName = $stateParams.subscriptionName;

        if ($stateParams.id === null || $stateParams.id === "") {
            vm.disablePolicyNameEntryField = false;
        } else {
            vm.disablePolicyNameEntryField = true;
        }

        activate();

        function activate() {
            if (initialData) {
                vm.policy = initialData.policy;
                vm.lookupPath = initialData.lookupPath;
            }

            fromPolicy();

            if (vm.lookupPath) {
                preselectLookupPath();
            }
        }

        function buttonVisible(item) {
            var node = item.$modelValue || item;
            if (node.type === 'condition') {
                return false; // no buttons visible for conditions
            }
            else if (_.includes(['not', 'if'], node.title)) {
                return node.nodes.length === 0;
            }
            return true;
        }

        function deletePolicy() {
            dialogs.confirm('Are you sure you want to delete this Policy?', 'Delete?', ['Yes', 'No']).then(function () {
                ascApi.deletePolicy($stateParams.subscriptionId, vm.policy.name).then(function (data) {
                    $state.go('manage-policy-list');
                });
            });
        }

        function lookupPathChecked(node) {
            if (node.lookupPath && vm.lookupPath && vm.lookupPath !== 'if') {
                node.lookupPath = false;
                var warningMsg =
                    'You already have another condition designated for lookup. ' +
                    'You must remove that before you can mark this one.';
                toastr.warning(warningMsg, 'Invalid');
            } else {
                toPolicy();
            }
        }

        function newConditionSubItem(scope) {
            var nodeData = scope.$modelValue || scope;
            var newItem = {
                id: nodeData.id * 10 + nodeData.nodes.length,
                title: 'condition',
                type: 'condition',
                nodes: []
            };
            nodeData.nodes.push(newItem);
        }

        function newOperatorSubItem(scope, operator) {
            var nodeData = scope.$modelValue || scope;
            var newItem = {
                id: nodeData.id * 10 + nodeData.nodes.length,
                title: operator,
                type: 'operator',
                nodes: []
            };
            nodeData.nodes.push(newItem);
        }

        function preview() {
            toPolicy();
            dialogs.openJsonModal(vm.policy, 'Policy Preview');
        }

        function save() {
            toPolicy();

            var policyDefinition = { policy: vm.policy };
            if (vm.lookupPath !== 'if') {
                policyDefinition.lookupPath = vm.lookupPath;
            }

            ascApi.savePolicy($stateParams.subscriptionId, vm.policy.name, policyDefinition).then(function (data) {
                if (data.error) {
                    console.log('Error while saving policy!', data);
                    toastr.error('Unexpected error while saving.', 'Error');
                } else {
                    vm.policy = data.policy;
                    toastr.success('Policy saved successfully.', 'Success');
                }
            });

            $state.go('manage-policy-list');
        }

        function toPolicy() {
            // Reset path
            vm.lookupPath = 'if';
            var path = vm.lookupPath;
            parseNode(vm.root.nodes[0], vm.policyItem.if, path);
            vm.policy.name = _.kebabCase(vm.policy.name);
            vm.policy.properties.policyRule.if = vm.policyItem.if;
        }

        //#region Private Methods
        function fromPolicy() {
            parsePolicyNode(vm.policy.properties.policyRule.if, vm.root);
        }

        function getEmptyPolicy() {
            return {
                name: '',
                properties: {
                    policyType: 'Custom',
                    description: '',
                    policyRule: {
                        if: {},
                        then: {
                            effect: 'deny'
                        }
                    }
                }
            };
        }

        function parseNode(node, currProperty, path, index) {
            //console.log('***parseNode', node, currProperty);
            if (node.type === 'operator') {
                var newItem = (node.title === 'not' ? {} : []);
                if (_.isArray(currProperty)) {
                    path += '[' + index + '].' + node.title;
                    var newLeaf = {};
                    newLeaf[node.title] = newItem;
                    currProperty.push(newLeaf);
                } else {
                    path += '.' + node.title;
                    currProperty[node.title] = newItem;
                }

                _.forEach(node.nodes, function (nd, idx) {
                    parseNode(nd, newItem, path, idx);
                });
            } else { // condition
                var targetFieldName = (node.target === 'action') ? 'source' : 'field';
                var targetValue = (node.target === 'tags.*' ? 'tags.' + node.tagsField : node.target);
                var condValue = (node.operator === 'in' && _.isString(node.value)) ? _.map(node.value.split(','), _.trim) : node.value;
                if (_.isArray(currProperty)) {
                    path += '[' + index + '].' + node.operator;
                    var newArrayItem = {};
                    newArrayItem[targetFieldName] = targetValue;
                    newArrayItem[node.operator] = condValue;
                    currProperty.push(newArrayItem);
                } else {
                    path += '.' + node.operator;
                    currProperty[targetFieldName] = targetValue;
                    currProperty[node.operator] = condValue;
                }
                if (node.lookupPath) {
                    vm.lookupPath = path;
                }
            }
        }

        function parsePolicyNode(node, currProperty) {
            //console.log('**parsePolicyNode', node, currProperty);
            var keys = _.keys(node);
            if (keys.length === 0) {
                return;
            }
            var isOperatorNode = keys.length === 1 && _.includes(['not', 'allOf', 'anyOf'], keys[0]);

            if (isOperatorNode) {
                var operatorKey = keys[0];
                var newOperator = {
                    title: operatorKey,
                    type: 'operator',
                    nodes: []
                };
                currProperty.nodes.push(newOperator);

                if (_.isArray(node[operatorKey])) {
                    _.forEach(node[operatorKey], function (item) {
                        parsePolicyNode(item, newOperator);
                    });
                } else {
                    parsePolicyNode(node[operatorKey], newOperator);
                }
            } else { // condition
                if (keys.length !== 2) {
                    throw 'Invalid Condition! Should only have 2 keys. Detected: ' + keys.length;
                }
                var operatorIndex = _.includes(['field', 'source'], keys[0]) ? 1 : 0;
                var target = node.field || node.source;
                var hasTagsField = (target.indexOf('tags.') === 0);
                var newCondition = {
                    title: 'condition',
                    type: 'condition',
                    target: (hasTagsField ? 'tags.*' : target),
                    tagsField: (hasTagsField ? target.substr(5) : null),
                    operator: keys[operatorIndex],
                    value: node[keys[operatorIndex]],
                    nodes: []
                };
                currProperty.nodes.push(newCondition);
            }
        }

        function preselectLookupPath() {
            var parts = vm.lookupPath.split('.');
            var nodeParts = [];
            for (var i = 0; i < parts.length - 1; i++) {
                var part = parts[i];
                var index = '0';
                var matches = part.match(/\[(.*?)\]/);
                if (matches) {
                    index = matches[1];
                }
                nodeParts.push('nodes[' + index + ']');
            }
            if (nodeParts.length !== 0) {
                var nodeLookup = _.join(nodeParts, '.');
                var node = _.get(vm.root, nodeLookup);
                node.lookupPath = true;
            }
        }
        //#endregion
    }
})();
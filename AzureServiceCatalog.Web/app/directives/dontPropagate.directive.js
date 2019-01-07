(function () {
    'use strict';

    angular.module('ascApp').directive('dontPropagate', dontPropagate);

    dontPropagate.$inject = ['$window'];
    // Adding a comment to the custom angular directive "dontPropagate" with a text editor, in this case Visual Studio Code.
    // Git will subsequently recognize, through watching the filesystem, that a change on this file has taken place, so that
    // I can push this change to my new branch: Adam-TestBranch
    function dontPropagate($window) {
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            element.bind(attrs.dontPropagate, function (e) {
                e.stopPropagation();
            });

        }
    }
})();
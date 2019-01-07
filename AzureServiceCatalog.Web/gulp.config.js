module.exports = function () {
    var app = 'app/';
    var homeViews = 'views/home/';

    var config = {
        homeViews: homeViews,
        index: homeViews + 'index.cshtml',
        appJs: app + '/',

        js: [
            app + '**/*.js'
        ],

        bower: {
            json: require('./bower.json'),
            directory: './lib/',
            ignorePath: '../..'
        },

        buildDir: 'built',
        buildStagingDir: 'build-staging',

        /**
         * optimized files
         */
        optimized: {
            app: 'app.js',
            lib: 'lib.js'
        }
    };

    config.getWiredepDefaultOptions = function () {
        var options = {
            bowerJson: config.bower.json,
            directory: config.bower.directory,
            ignorePath: config.bower.ignorePath
        };
        return options;
    }

    return config;
};
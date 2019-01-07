'use strict';
var config = require('./gulp.config')();
var del = require('del');
var gulp = require('gulp');
var map = require('map-stream');
var $ = require('gulp-load-plugins')();
var _ = require('lodash');
var wiredep = require('wiredep');

gulp.task('lint', function () {
    return gulp.src(config.js)
        .pipe($.jshint())
        .pipe($.jshint.reporter(require('jshint-stylish')))
        //.pipe(reportErrorToAppveyor)
        .pipe($.size())
        .pipe(exitOnJshintError);
});

var reportErrorToAppveyor = map(function(file, cb){
    if (!file.jshint.success) {
        //console.log(file.jshint.results);
        _.forEach(file.jshint.results, function (result) {
            // TODO: log compilation message to AppVeyor Build Worker API
            console.log(result.file);
            console.log(result.error.line, result.error.character, result.error.reason);
        });
    }
});

var exitOnJshintError = map(function (file, cb) {
    if (!file.jshint.success) {
        console.error('jshint failed! View log for details.');
        process.exit(1);
    }
});

gulp.task('optimize', ['wiredep'], function () {
    console.log('Optimizing...');

    var assets = $.useref({ searchPath: './' });
    // Filters are named for the gulp-useref path
    //var cssFilter = $.filter('**/*.css');
    //var jsAppFilter = $.filter(['**/' + config.optimized.app]);
    //var jsLibFilter = $.filter('**/' + config.optimized.lib);

    return gulp
        .src(config.index)
        .pipe($.plumber())
        .pipe(assets)
        .pipe($.if('*.js', $.uglify()))
        .pipe(gulp.dest(config.buildDir));
});

gulp.task('clean', function () {
    var dirsToClean = [].concat(config.buildDir, config.buildStagingDir);
    console.log('Cleaning: ' + dirsToClean);
    return del(dirsToClean);
});

gulp.task('wiredep', ['clean'], function () {
    //console.log(config);
    var options = config.getWiredepDefaultOptions();
    var wiredep = require('wiredep').stream;
    var appScripts = gulp.src(config.js).pipe($.angularFilesort());

    return gulp
        .src(config.index)
        .pipe(wiredep(options))
        .pipe($.inject(appScripts))
        .pipe(gulp.dest(config.homeViews));
});
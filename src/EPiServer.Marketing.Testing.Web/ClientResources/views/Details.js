﻿define([
    "dojo/_base/declare",
    "epi/dependency",
    "dojo/dom",
    "dojo/ready",
    "dijit/registry",
    "dojo/dom-style",
    "dojo/topic",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!marketing-testing/views/Details.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "epi/datetime",
    "epi/username",
    "dojo/dom-class",
    "dojo/query",
    "marketing-testing/scripts/abTestTextHelper",
    "marketing-testing/scripts/thumbnails",
    "dojox/layout/ContentPane",
    "dojo/fx",
    "dojo/dom-construct",
    "xstyle/css!marketing-testing/css/ABTesting.css",
    "dijit/form/DropDownButton",
    "dijit/TooltipDialog",
    "dijit/form/Button",
    "dijit/ProgressBar"

], function (
    declare,
    dependency,
    dom,
    ready,
    registry,
    domStyle,
    topic,
    widgetBase,
    templatedMixin,
    widgetsInTemplateMixin,
    template,
    resources,
    datetime,
    username,
    domClass,
    query,
    textHelper,
    thumbnails,
    ContentPane,
    CoreFX,
    DomConstruct
) {
    return declare([widgetBase, templatedMixin, widgetsInTemplateMixin],
        {
            templateString: template,
            resources: resources,
            kpiSummaryWidgets: new Array(),

            constructor: function () {
                var contextService = dependency.resolve("epi.shell.ContextService"), me = this;
                me.context = contextService.currentContext;
                me.subscribe("/epi/shell/context/changed", me._contextChanged);
            },

            _contextChanged: function (newContext) {
                var me = this;
                if (!newContext || newContext.type !== 'epi.marketing.testing') {
                    return;
                };
                if (this.context.data.test.kpiInstances.length > 1) {
                    this._setToggleAnimations();
                    this.summaryToggle.style.visibility = "visible"
                } else {
                    this.summaryToggle.style.visibility = "hidden"
                }
                this._resetView();
            },

            startup: function () {
                var contextService = dependency.resolve("epi.shell.ContextService"), me = this;

                this.context = contextService.currentContext;
                this._displayOptionsButton(this.context.data.userHasPublishRights);


                if (document.getElementById("draftThumbnaildetail")) {
                    document.getElementById("publishThumbnaildetail-spinner").style.display = "block";
                    document.getElementById("draftThumbnaildetail-spinner").style.display = "block";
                    document.getElementById("publishThumbnaildetail").style.display = "none";
                    document.getElementById("draftThumbnaildetail").style.display = "none";
                }

                if (this.context.data.test.kpiInstances.length > 1) {
                    this._setToggleAnimations();
                    this.summaryToggle.style.visibility = "visible"
                } else {
                    this.summaryToggle.style.visibility = "hidden"
                }

                textHelper.initializeHelper(this.context, resources.detailsview);
                this._displayOptionsButton(this.context.data.userHasPublishRights);
                this._renderData();
            },

            _setToggleAnimations: function () {
                var me = this;
                this.controlSummaryOut = CoreFX.wipeOut({
                    node: me.controlDetailsSummaryNode,
                    rate: 15,
                    onBegin: function () { me.summaryToggle.innerHTML = me.resources.detailsview.show_summary }
                });

                this.controlSummaryIn = CoreFX.wipeIn({
                    node: me.controlDetailsSummaryNode,
                    rate: 15,
                    onBegin: function () { me.summaryToggle.innerHTML = me.resources.detailsview.hide_summary }
                });

                this.challengerSummaryOut = CoreFX.wipeOut({
                    node: me.challengerDetailsSummaryNode,
                    rate: 15
                });

                this.challengerSummaryIn = CoreFX.wipeIn({
                    node: me.challengerDetailsSummaryNode,
                    rate: 15
                });
            },

            _onPickWinnerOptionClicked: function () {
                var me = this;
                this.kpiSummaryWidgets = new Array();
                me.contextParameters = {
                    uri: "epi.marketing.testing:///testid=" + this.context.data.test.id + "/pickwinner"
                };
                topic.publish("/epi/shell/context/request", me.contextParameters, { sender: this });
            },

            _onAbortOptionClicked: function () {
                if (confirm(resources.detailsview.abort_confirmation_message)) {
                    this.kpiSummaryWidgets = new Array();
                    var me = this, store = this.store || dependency.resolve("epi.storeregistry").get("marketing.abtesting");
                    store.remove(this.context.data.test.originalItemId);
                    me.contextParameters = {
                        uri: "epi.cms.contentdata:///" + this.context.data.draftVersionContentLink
                    };
                    topic.publish("/epi/shell/context/request", me.contextParameters);
                }
            },

            _onCancelClick: function () {
                var me = this;
                this.kpiSummaryWidgets = new Array();
                this._resetView();
                me.contextParameters = {
                    uri: "epi.cms.contentdata:///" + this.context.data.latestVersionContentLink
                };
                topic.publish("/epi/shell/context/request", me.contextParameters);
            },

            _onControlViewClick: function () {
                var me = this;
                me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.publishedVersionContentLink };
                topic.publish("/epi/shell/context/request", me.contextParameters);
            },

            _onChallengerViewClick: function () {
                var me = this;
                me.contextParameters = { uri: "epi.cms.contentdata:///" + this.context.data.draftVersionContentLink };
                topic.publish("/epi/shell/context/request", me.contextParameters);
            },

            _renderDataVariables: function () {
                var me = this;
                console.log(this.context.data.test.originalItemId);
                return this.context.data.test.originalItemId;
            },

            _renderData: function () {
                var me = this;
                var summaryWidget;
                textHelper.renderTitle(this.title);
                this.myiframe.src = "/EPiServer/EPiServer.Marketing.Testing/ExperimentResult?fs_FlagKey=" + this.context.data.test.fs_FlagKey + "&fs_ExperimentKey=" + this.context.data.test.fs_ExperimentKey;
                textHelper.renderTestStatus(this.testStatus, this.testStarted);
                textHelper.renderTestGloballyDisabled(this.testGloballyDisabled, this.detailsNotificationBar);
                //textHelper.renderTestDuration(this.testDuration);
                //textHelper.renderTestRemaining(this.testRemaining, this.testRemainingText);
                //textHelper.renderDurationProgress(durationProgressBarDetails);
                //textHelper.renderConfidence(this.confidence, this.context.data.test);
                textHelper.renderPublishedInfo(this.publishedBy, this.datePublished);
                textHelper.renderDraftInfo(this.changedBy, this.dateChanged);
                this.kpiSummaryWidgets.push(textHelper.renderControlSummary(this.controlDetailsSummaryNode, this.controlConversionPercent));
                this.kpiSummaryWidgets.push(textHelper.renderChallengerSummary(this.challengerDetailsSummaryNode, this.challengerConversionPercent));
                textHelper.renderDescription(this.testDescription);
                console.log("this.participationPercentage" + this.participationPercentage);
                console.log("this.totalParticipants" + this.totalParticipants);
                textHelper.renderVisitorStats(this.participationPercentage, this.totalParticipants);
                ready(function () {

                    pubThumb = document.getElementById("publishThumbnaildetail");
                    draftThumb = document.getElementById("draftThumbnaildetail");
                    if (me.context.customViewType == "marketing-testing/views/details") {
                        thumbnails._setThumbnail(pubThumb, me.context.data.publishPreviewUrl);
                        thumbnails._setThumbnail(draftThumb, me.context.data.draftPreviewUrl);
                    };
                    me._renderKpiMarkup("details_conversionMarkup");
                    for (x = 0; x < me.kpiSummaryWidgets.length; x++) {
                        me.kpiSummaryWidgets[x].startup();
                    }
                });
                this.renderStatusIndicatorStyles();
                this._resetView();
            },

            _renderKpiMarkup: function (conversionMarkupId) {
                var kpiuiElement = dom.byId(conversionMarkupId);
                this._clearKpiMarkup(kpiuiElement);

                for (var x = 0; x < this.context.data.test.kpiInstances.length; x++) {
                    var friendlyName = (this.context.data.test.kpiInstances[x].friendlyName == "Landing Page" ? "Page View" : this.context.data.test.kpiInstances[x].friendlyName);
                    var goalsFriendlyName = DomConstruct.toDom("<label class='epi-kpiLabel-bold'>" +  friendlyName  + "</label>");
                    var goalsDescription = DomConstruct.toDom("<P>" + this.context.data.test.kpiInstances[x].description + "</p>");

                    var goalsContent = new ContentPane({
                        content: this.context.data.test.kpiInstances[x].uiReadOnlyMarkup
                    }).placeAt(kpiuiElement);
                    dojo.place(goalsFriendlyName, goalsContent.containerNode, "first");
                    dojo.place(goalsDescription, goalsContent.containerNode, "last");
                }
            },

            _clearKpiMarkup: function (conversionMarkupElement) {
                if (conversionMarkupElement) {
                    var contentPane = dojo.query('#details_conversionMarkup > *');
                    if (contentPane[0]) {
                        dojo.forEach(dijit.findWidgets(contentPane)), function (w) {
                            w.destroyRecursive();
                        };
                        var dijitContentPane = dijit.byId(contentPane[0].id);
                        dijitContentPane.destroy();
                        conversionMarkupElement.innerHTML = "";
                    }
                }
            },

            _clearKpiDescription: function (conversionMarkupElement) {
                if (conversionMarkupElement) {
                    var contentPane = dojo.query('#details_kpidescription > *');
                    if (contentPane[0]) {
                        dojo.forEach(dijit.findWidgets(contentPane)), function (w) {
                            w.destroyRecursive();
                        };
                        var dijitContentPane = dijit.byId(contentPane[0].id);
                        dijitContentPane.destroy();
                        conversionMarkupElement.innerHTML = "";
                    }
                }
            },

            _displayOptionsButton: function (show) {
                var dropDownButton = registry.byId("optionsDropdown");
                var pickWinnerOption = registry.byId("pickWinnerMenuItem");
                if (show) {
                    //If the test is not running, disable the pick a winner option item
                    if (this.context.data.test.state === 0) {
                        pickWinnerOption.set("disabled", true);
                    } else {
                        pickWinnerOption.set("disabled", false);
                    }
                    domStyle.set(dropDownButton.domNode, "visibility", "visible");
                    dropDownButton.startup(); //Avoids conditions where the widget is rendered but not active.
                } else {
                    domStyle.set(dropDownButton.domNode, "visibility", "hidden");
                }
            },

            renderStatusIndicatorStyles: function () {
                var me = this;
                me.baseWrapper = "cardWrapper";
                if (this.context.data.test.state < 2) {
                    me.statusIndicatorClass = "leadingContent";
                }
                else { me.statusIndicatorClass = "winningContent"; }

                if (textHelper.publishedPercent > textHelper.draftPercent) {
                    this.controlStatusIcon.title = resources.detailsview.content_winning_tooltip;
                    this.challengerStatusIcon.title = "";
                    domClass.replace(this.controlStatusIcon, me.statusIndicatorClass);
                    domClass.replace(this.challengerStatusIcon, "noIndicator");
                    domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlLeaderBody");
                    domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerDefaultBody");
                }
                else if (textHelper.publishedPercent < textHelper.draftPercent) {
                    this.controlStatusIcon.title = "";
                    this.challengerStatusIcon.title = resources.detailsview.content_winning_tooltip;
                    domClass.replace(this.controlStatusIcon, "noIndicator");
                    domClass.replace(this.challengerStatusIcon, me.statusIndicatorClass);
                    domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlTrailingBody");
                    domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerLeaderBody");
                }
                else {
                    this.controlStatusIcon.title = "";
                    this.challengerStatusIcon.title = "";
                    domClass.replace(this.controlStatusIcon, "noIndicator");
                    domClass.replace(this.challengerStatusIcon, "noIndicator");
                    domClass.replace(this.controlWrapper, me.baseWrapper + " 2column epi-abtest-preview-left-side controlDefaultBody");
                    domClass.replace(this.challengerWrapper, me.baseWrapper + " 2column epi-abtest-preview-right-side challengerDefaultBody");
                }
            },

            _generateThumbnail: function (previewUrl, canvasId, parentContainerClass) {
                var pubThumb = dom.byId(canvasId);

                if (pubThumb) {
                    pubThumb.height = 768;
                    pubThumb.width = 1024;
                    rasterizehtml.drawURL(previewUrl, pubThumb, { height: 768, width: 1024 }).then(
                        function success(renderResult) {
                            query('.' + parentContainerClass).addClass('hide-bg');
                        });
                }
            },

            _toggleSummaries: function (evt) {
                if (this.summaryToggle.innerHTML === this.resources.detailsview.hide_summary) {
                    this.controlSummaryOut.play();
                    this.challengerSummaryOut.play();
                }
                else {
                    this.controlSummaryIn.play();
                    this.challengerSummaryIn.play();
                }
            },

            _resetView: function () {
                var abTestBody = dom.byId("detailsAbTestBody");
                var abToolBar = dom.byId("detailsToolbarGroup");
                if (abTestBody) {
                    abTestBody.scrollIntoView(true);
                    abToolBar.scrollIntoView(true);
                }
            }
        });
});
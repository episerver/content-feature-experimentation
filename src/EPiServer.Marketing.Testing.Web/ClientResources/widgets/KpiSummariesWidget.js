﻿define([
    "dojo/_base/declare",
    "dijit/_WidgetBase",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dojo/text!./templates/KpiSummariesWidget.html",
    "epi/i18n!marketing-testing/nls/abtesting",
    "dojo/dom",
    "dojo/dom-construct",
    "dojox/layout/ContentPane",
    "marketing-testing/widgets/WeightSelector",
    "marketing-testing/widgets/KpiSummaryRow",
    "xstyle/css!marketing-testing/css/KpiWidget.css"
], function (declare, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, template, resources, dom, domConstruct, ContentPane, weightSelector, kpiSummaryRow) {

    return declare("KpiSummariesWidget", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin], {

        id: this.id,

        templateString: template,

        resources: resources,

        //requires being set to an array of objects
        //objects need to contain name, conversion, weight (Low, Medium, High), and performance value
        kpis: null,

        startup: function () {
            var trInsertNode = dom.byId(this.id + "Summaries");
            var templatedRow;
            if (trInsertNode) {
                trInsertNode.innerHTML = "";
                for (var x = 0; x < this.kpis.length; x++) {
                    templatedRow = new KpiSummaryRow({
                        name: this.kpis[x].markup,
                        conversions: this.kpis[x].conversions,
                        performance: this.kpis[x].performance
                    })

                    domConstruct.place(templatedRow.domNode, trInsertNode);
                    new weightSelector({
                        value: this.kpis[x].weight,
                        disabled: true,
                        showLabel: false
                    }).placeAt(templatedRow.id + "weight");

                    new ContentPane({
                        content: this.kpis[x].markup
                    }).placeAt(templatedRow.id + "markup");
                }
            }
        }
    });
});
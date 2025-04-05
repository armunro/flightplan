import {createApp} from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js'
import FpTimer from "../../components/Timer.js"
import FpCalEvent from "../../components/CalEvent.js"
import FpWaypoint from "../../components/Waypoint.js"
import FpMessage from "../../components/Message.js"
import {ToneGenerator} from "../js/ToneGenerator.js"
import Helpers from "../../components/Helpers.js";
import JtsTopicViewer from "../../components/TopicViewer.js"
import FpWaypointEditor from "../../components/WaypointEditor.js"
import FpTaskList from "../../components/TaskList.js"

let jts;

let app = createApp({
    data() {
        return {
            api: null,
            aceEditor: null,
            editing: {
                "object": {
                    "id": '',
                    "instance": {}
                },
                "kind": {
                    "id": '',
                    "instance": {}
                }
            },
            toneGenerator: new ToneGenerator(),
            subs: [
                {"name": "tasks", "kindPattern": "Task"},
                {"name": "timers", "kindPattern": "Timer"},
                {"name": "messages", "kindPattern": "Message"},
            ],
            activePlan: {contents: {waypoints: []}},
            kindFilter: '*',
            date: '',
            componentKey: 0,
            jetstreamUri: "https://jtstrm.xyz",
            activeScreenTrigger: "",
            activeScreen: "",
            jetObjects: {},
            globalKinds: [],
           
            objects: {
                "contexts": [],
                "events": [],
                "tasks": [],
                "timers": [],
                "messages": [],
                "poms": [{"contents": {}}]
            },
            advisorObjects: {},
            advised: {},
            scratch: '',
            objectScope: '',
            panels: {0: null, 1: null},
            activePanel: 0,
            planner: {
                active: {
                    plan: {contents: {}},
                    waypoints: [],
                    waypoint: ''
                },
                entering: {
                    activePlanId: '',
                    activeWpt: 'XXX',
                    contact: {
                        type: "type",
                        title: "title"
                    },
                    waypoint: {
                        contents: {
                            icon: "喝",
                            key: "WP1",
                            summary: "WAYPOINT SUMMARY",
                            date: "2022-10-3"
                        }
                    },

                },

            },           
            selectedWpt: '',
            auth: {
                token: '',
                ident: '',
                secret: ''
            },
            heartbeat: {
                status: 0,
                date: new Date(),
                ping: -1
            },
            stats: {
                tx: 0,
                rx: 0
            }
        };
    },
    methods: {
        loadAce(contents) {
            if (this.aceEditor == null) {
                ace.require("ace/ext/language_tools");
                this.aceEditor = ace.edit("aceEditor");
                this.aceEditor.setOptions({
                    fontSize: 13,
                    fontFamily: "JetBrains Mono Regular NerdFont Complete",
                    enableBasicAutocompletion: true,
                    copyWithEmptySelection: true
                });
                this.aceEditor.session.setMode("ace/mode/json");
                this.aceEditor.session.setOption("enableBasicAutocompletion", true)
                this.aceEditor.setTheme("ace/theme/one_dark");
                this.aceEditor.setShowPrintMargin(false);
                this.aceEditor.setHighlightActiveLine(false);
            }
            this.aceEditor.setValue(contents)
        },
        getTasks: function () {
            jts.getTasks().then(x => app.objects.tasks = x);
        },
        activateEnteredPlan: function () {
            jts.getObject(app.planner.entering.activePlanId).then(x => {
                app.activePlan = x
                app.planner.active.plan = x;
                jts.getObjects("Waypoint").then(x => app.planner.active.waypoints = x)
            })
        },
        getToken: function () {
            jts.getToken(app.auth.ident, app.auth.secret).then(response => app.auth.token = response.token)
        },
        getHeartbeat: function () {
            jts.getStatus().then(resp => {
                app.heartbeat.ping = jts.latency;
                app.heartbeat.status = resp.status;
                app.heartbeat.date = new Date();
            })
            app.stats.tx = jts.txCount;
            app.stats.rx = jts.rxCount;

        },

        getKind: function (id) {
            jts.getPrototype(id).then(x => {
                this.editing.object.id = null
                this.editing.object.instance = {}
                this.loadAce(JSON.stringify(x.contents, undefined, 4))
                app.activatePage('fp-object-edit', 1, false);
            })
        },
        setWpt: function () {
            app.planner.active.plan.contents.activeWaypoint = app.planner.entering.activeWpt;
        },
        saveActivePlan: function () {
            jts.editObject(app.activePlan.id, JSON.stringify(app.activePlan.contents));
            app.activateEnteredPlan();
        },
        getObjects: function () {
            jts.getObjects(app.kindFilter).then(x => app.jetObjects = x);
        },

        refreshSubs: function () {
            app.subs.forEach(x => this.refreshSub(x));
        },
        reinit: function () {
            this.getHeartbeat();
            this.getGlobalKinds();
            this.getObjects();  
            this.refreshSubs();
            this.getTasks();
        },
        getGlobalKinds: function () {
            jts.getKindSummary().then(json => app.globalKinds = json);
        },

        getFilteredKinds: function () {
            return this.globalKinds.filter((x) => x.toLowerCase().includes(app.kindManager.displayFilter.toLowerCase()));
        },
        refreshSub: function (sub) {
            jts.getObjects(sub.kindPattern).then(y => app.objects[sub.name] = y);
        },
        adviseObject: function (object, parent = null) {
            app.advisorObjects = dotNotate(object)
            app.advised = object;
            if (parent) {
                app.parentAdvisorObjects = dotNotate(parent)
                app.parentAdvised = parent
            } else {
                app.parentAdvisorObjects = null
                app.parentAdvised = null;
            }
        },
        clearAdvisor: function () {
            app.advisorObjects = {};
            app.advised = {};
        },
        

        editKind: function (kind) {
            app.editing.kind.id = kind.kindId;
            app.editing.kind.instance = kind;
        },
        
        clearKind: function () {
            app.editing.kind.id = '';
            app.editing.kind.instance = {};
        },

        deleteKind: function () {
            jts.deleteKind(app.editing.kind.id).then(x => app.getGlobalKinds());
        },

        commitKind: function () {
            jts.createKind(app.editing.kind.id).then(x => this.getGlobalKinds());
        },
        handler: function (e) {
            alert(e);
        },
        createObjectFromPrototype: function () {
            this.getKind(app.objectManager.entering.prototypeId)
        },
        editObject: function (object) {
            this.editing.object.id = object.id;
            this.editing.object.instance = object
            let contents = JSON.stringify(object.contents, undefined, 4);
            this.loadAce(contents);
            app.activatePage('fp-object-edit', 1, false);
        },

        filterObjects(kind) {
            app.kindFilter = kind;
            this.getObjects();
            this.activatePage('fp-object', 0)
        },

        deleteObject: function () {
            jts.deleteObject(app.editing.object.id).then(x => {
                app.reinit()
            });
        },

        createObject() {
            let kind = document.getElementById("objectKindInput").value;
            let contents = this.aceEditor.getValue();
            jts.createObject(kind, contents).then((x) => {
                app.reinit();
                app.editObject(x)
            });
        },
        saveObject() {
            let contents = this.aceEditor.getValue();
            jts.editObject(app.editing.object.id, contents).then(x => app.reinit());
        },


        activatePage: function (page, panelNum = 0, hidePrev = true) {
            //if panelNum < 0 : intent any avail
            let pageActivatingElem = document.getElementById(page)
            //STEP determine destination panel            
            let activePanelElem = document.getElementById("screen-panel-" + panelNum);

            //STEP deactivate pages on the now-active panel
            if (this.panels[panelNum]) {
                let pageRemovingElem = this.panels[panelNum];
                pageRemovingElem.parentNode.removeChild(pageRemovingElem);
                document.getElementById("mfd-page").appendChild(pageRemovingElem);
            }
            //STEP add activating page to now-active panel
            this.panels[panelNum] = pageActivatingElem
            activePanelElem.appendChild(pageActivatingElem);
            pageActivatingElem.classList.add("show");
        },


        padStartWithZeros(num, size) {
            let s = String(num);
            while (s.length < (size || 2)) {
                s = "0" + s;
            }
            return s;
        },
        setTime: function () {
            const today = new Date();
            this.date = today.getFullYear() + "-" +
                (today.getMonth() + 1) + "-" + today.getDate() + " " +
                today.getHours() + ":" +
                Helpers.formatTime(today.getMinutes()) + ":" +
                Helpers.formatTime(today.getSeconds());
        }
    },
    mounted() {
        if (localStorage.jet_token) {
            this.auth.token = localStorage.jet_token;
        }
        if (localStorage.jetstreamUri) {
            this.jetstreamUri = localStorage.jetstreamUri;
        }
        if (localStorage.activePlanId) {
            this.planner.entering.activePlanId = localStorage.activePlanId;
        }


        jts = new JetstreamApi(this.jetstreamUri, this.auth.token)
        this.api = jts;
        setInterval(() => this.setTime(), 1000);
        setInterval(() => this.getHeartbeat(), 5000);
        setTimeout(() => {
                this.reinit()
                if (this.planner.entering.activePlanId) {
                    this.activateEnteredPlan();
                }
                this.activatePage('fp-kind', 0, true)
            }
            , 20)
    },
    watch: {
        "auth.token"(newToken) {
            localStorage.jet_token = newToken;
            jts.token = newToken;
        },
        "planner.entering.activePlanId"(newPlanId) {
            localStorage.activePlanId = newPlanId;
        },
        jetstreamUri(newUri) {
            localStorage.jetstreamUri = newUri;
        }
    }
});


app.component("fptimer", FpTimer);
app.component("fpcalevent", FpCalEvent);
app.component("fpwaypoint", FpWaypoint);
app.component("fpmessage", FpMessage);
app.component("jtstopicviewer", JtsTopicViewer);
app.component("fpwaypointeditor", FpWaypointEditor);
app.component("fptasklist", FpTaskList);
app = app.mount("#app");
document.app = app;
document.jts = jts;

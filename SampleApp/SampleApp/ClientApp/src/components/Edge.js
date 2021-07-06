import React, { Component } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export class Edge extends Component {
    static displayName = Edge.name;

    constructor(props) {
        super(props);
        this.state = {
            pipelineTopologies: [],
            livePipelines: [],
            loading: true,
            loadingPipelineTopologies: true,
            loadingLivePipelines: true,
            behindProxy: false,
            videoName: "",
            livePipelineName: "",
            rtspUrl: "",
            rtspUsername: "",
            rtspPassword: "",
            livePipelineTopologyName: "",
            livePipelineState: "inactive",
            livePipelineEnabled: false,
            pipelineTopologiesEnabled: false,
            connection: null,
            events: []
        };
        this.token = null;
        this.deletePipelineTopology = this.deletePipelineTopologyOperation.bind(this);
        this.createPipelineTopology = this.createPipelineTopologyOperation.bind(this);
        this.createLivePipeline = this.createLivePipelineOperation.bind(this);
        this.deleteLivePipeline = this.deleteLivePipelineOperation.bind(this);
        this.changeStateLivePipeline = this.changeStateLivePipelineOperation.bind(this);
    }

    async componentDidMount() {
        await this.getPipelinesTopologies();
        await this.getLivePipelines();
        await this.initConnection();
    }

    async initConnection() {
        const connection = new HubConnectionBuilder()
            .withUrl("/eventhub")
            .configureLogging(LogLevel.Error)
            .build();

        connection.on("ReceivedNewEvent", (eventData) => {
            const { events } = this.state;

            events.push(eventData);
            console.log('Added event');

            this.setState({ events: events});
        });

        connection.start();
        this.setState({ connection: connection });
    }

    async getToken() {
        const response = await fetch('Auth/GetToken', {
            method: 'GET'
        });

        this.token = await response.text();
    }

    async deleteLivePipelineOperation(livePipeline) {
        const token = this.token;
        const url = '';
        try {
            const response = await fetch(url, {
                method: 'DELETE',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (response.ok) {
                await this.getLivePipelines();
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot delete livepipeline: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async deletePipelineTopologyOperation(pipelineTopologyName) {
        const url = `/VideoAnalyzer/PipelineTopologyDelete?pipelineTopologyName=${pipelineTopologyName}`;
        try {
            const response = await fetch(url, {
                method: 'DELETE'
            });

            if (response.ok) {
                await this.getPipelinesTopologies();
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot delete pipelineTopology: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async createPipelineTopologyOperation(event) {
        event.preventDefault();
        const { pipelineTopologyName, behindProxy } = this.state;

        const url = `/VideoAnalyzer/PipelineTopologySet?pipelineTopologyName=${pipelineTopologyName}`;
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    "Content-Type": "application/json"
                }
            });

            if (response.ok) {
                this.setState({ pipelineTopologyName: "", videoName: "", behindProxy: false }, async () =>
                    await this.getPipelinesTopologies());
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot create tje pipelineTopology: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async createLivePipelineOperation(event) {
        event.preventDefault();
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName } = this.state;
        const url = `/VideoAnalyzer/LivePipelineSet?pipelineTopologyName=${livePipelineTopologyName}&livePipelineName=${livePipelineName}&username=${rtspUsername}&password=${rtspPassword}&url=${rtspUrl}`;
        
        try {
            const response = await fetch(url, {
                method: 'POST'
            });

            if (response.ok) {
                this.setState({ livePipelineName: "", rtspUrl: "", rtspUsername: "", rtspPassword: "", livePipelineTopologyName: "", videoName: "" },
                    async() => await this.getLivePipelines());           
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot create livepipeline: ${errorMessageObj.error.message}`);
            }
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async checkStatus(asyncOpUrl) {
        const token = this.token;
        
        try {
            const asyncResponse = await fetch(asyncOpUrl, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            if (asyncResponse.ok) {
                const jsonResp = JSON.parse(await asyncResponse.text());
                if (jsonResp.status === "Running") {
                    return await this.checkStatus(asyncOpUrl);
                } else if (jsonResp.status === "Succeeded") {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                throw new Error(await asyncResponse.text());
            }
        }
        catch (e) {
            throw new Error(e);
        }
    }

    async changeStateLivePipelineOperation(livePipeline, state) {
        const action = state === "Inactive" ? "Activate" : "Deactivate";
        const url = `/VideoAnalyzer/LivePipeline${action}?livePipelineName=${livePipeline}`;
        
        try {
            const response = await fetch(url, {
                method: 'POST'
            });

            if (response.ok) {
                await this.getLivePipelines();
            }
            else {
                alert("Operation failed, please check the console log.");
                console.log(await response.text());
            }

            if (action === "Deactivate") {
                await this.stopToEvent();
                this.setState({ events: [] });
            }
            else {
                await this.listenToEvent();
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async getPipelinesTopologies() {
        const url = '/VideoAnalyzer/PipelineTopologyList';
        try {
            const response = await fetch(url, {
                method: 'GET'
            });

            var data = [];

            if (response.ok) {
                const jsonResponse = await response.json();
                data = jsonResponse;
            }
            else {
                console.log(response.statusText);
            }

            this.setState({ pipelineTopologies: data });
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingPipelineTopologies: false });
        }
    }

    async getLivePipelines() {
        const url = '/VideoAnalyzer/LivePipelineList';
        try {
            const response = await fetch(url, {
                method: 'GET'
            });

            var data = [];

            if (response.ok) {
                const jsonResponse = await response.json();
                data = JSON.parse(jsonResponse);
            }
            else {
                console.log(response.statusText);
            }

            this.setState({ livePipelines: data });
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async getVideoPlayback(videoName, pipelineName) {
        const token = this.token;
        const url = '';
        const authUrl = '';
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

            let tunneledRtspUrl = "";
            let playbackToken = "";
            if (response.ok) {
                const jsonResponse = await response.json();
                tunneledRtspUrl = jsonResponse.properties.streaming.rtspTunnelUrl;

                const responseAuth = await fetch(authUrl, {
                    method: 'POST',
                    headers: {
                        "Authorization": `Bearer ${token}`
                    }
                });

                if (responseAuth.ok) {
                    const jsonAuthResponse = await responseAuth.json();
                    playbackToken = jsonAuthResponse.token;
                }
                else {
                    const errorMessageObj = await responseAuth.json();
                    alert(`Cannot get video playback token: ${errorMessageObj.error.message}`);
                }
            }
            else {
                const errorMessageObj = await response.json();
                alert(`Cannot get video playback url: ${errorMessageObj.error.message}`);
            }

            this.renderVideoPlayer(tunneledRtspUrl, playbackToken, pipelineName);
        }
        catch (e) {
            console.log(e);
        }
        finally {
            this.setState({ loadingLivePipelines: false });
        }
    }

    async listenToEvent() {
        const url = '/VideoAnalyzer/ListenToEvents';
        try {
            const response = await fetch(url, {
                method: 'GET'
            });

            if (!response.ok) {
                console.log(response.statusText);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    async stopToEvent() {
        const url = '/VideoAnalyzer/StopListeningToEvents';
        try {
            const response = await fetch(url, {
                method: 'GET'
            });

            if (!response.ok) {
                console.log(response.statusText);
            }
        }
        catch (e) {
            console.log(e);
        }
    }

    setFormData(event) {
        const elementType = event.target.parentElement.parentElement.name;
        const value = event.target.type === "checkbox" ? event.target.checked : event.target.value;
        this.setState({
            ...this.state,
            [event.target.name]: value
        }, () => this.validate(elementType));
    }

    validate(elementType) {
        const { livePipelineName, rtspUrl, rtspUsername, rtspPassword, livePipelineTopologyName, videoName, pipelineTopologyName } = this.state;

        let isLivePipelineValid = false;
        let isPipelineTopologiesValid = false;

        if (elementType === "livepipeline") {
            isLivePipelineValid = livePipelineName.length > 0 && rtspUrl.length > 0 && rtspUsername.length > 0 && rtspPassword.length > 0 && livePipelineTopologyName.length > 0 && videoName.length > 0;
        }
        else {
            isPipelineTopologiesValid = pipelineTopologyName.length;
        }

        this.setState({
            livePipelineEnabled: isLivePipelineValid,
            pipelineTopologiesEnabled: isPipelineTopologiesValid
        });
    }

    renderPipelineTopologies() {
        const { pipelineTopologies } = this.state;
        return (
            <div>
                <h3>Pipeline Topologies</h3>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Parameters</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {
                            pipelineTopologies.map((data, index) =>
                                <tr key={index}>
                                    <td>{data.name}</td>
                                    <td>{data.properties.description}</td>
                                    <td>
                                        <ul>
                                            {data.properties.parameters.map((p,i) =>
                                                <li key={i}>{p.name}</li>
                                            )}
                                        </ul>
                                    </td>
                                    <td>
                                        <button className="btn btn-primary" onClick={() => this.deletePipelineTopology(data.name)}>Delete</button>
                                    </td>
                                </tr>
                            )}
                    </tbody>
                </table>

                <h5>Add new</h5>
                <form name="pipelinetopology" onSubmit={(e) => this.createPipelineTopology(e)}>
                    <fieldset>
                        <label>Behind proxy</label>&nbsp;<input type="checkbox" checked={this.state.behindProxy} name="behindProxy" onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="pipelineTopologyName" value={this.state.pipelineTopologyName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <button type="submit" disabled={!this.state.pipelineTopologiesEnabled}>Create</button>
                </form>
            </div>
        );
    }

    renderLivePipelines() {
        const { livePipelines } = this.state;
        return (
            <div>
                <h3>LivePipelines</h3>
                <table className='table table-striped' aria-labelledby="tabelLabel">
                    <tbody>
                        {
                            livePipelines.map((data, index) =>
                                <div>
                                <tr>
                                    <th>Name</th>
                                    <th>Description</th>
                                    <th>Topology</th>
                                    <th>State</th>
                                    <th>Parameters</th>
                                    <th>Action</th>
                                </tr>
                                <tr key={index}>
                                    <td>{data.name}</td>
                                    <td>{data.properties.description}</td>
                                    <td>{data.properties.topologyName}</td>
                                    <td>{data.properties.state}</td>
                                    <td>
                                        <ul>
                                            {data.properties.parameters.map((p, i) =>
                                                <li key={i}>{p.name}: <b>{p.value === undefined ? "**********" : p.value}</b></li>
                                            )}
                                        </ul>
                                    </td>
                                    <td>
                                        <button className="btn btn-primary" onClick={() => this.deleteLivePipeline(data.name)}>Delete</button><br /><br />
                                        {
                                            data.properties.state === "Inactive" ? (
                                                <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties.state)}>Activate</button>
                                            )
                                            :
                                            (
                                                <div>
                                                    <button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, data.properties.state)}>Deactivate</button><br /><br />
                                                            <button className="btn btn-primary" onClick={() => this.getVideoPlayback(data.properties.parameters.find(x => x.name === "videoNameParameter").value, data.name)}>Play video</button>
                                                            {/*<button className="btn btn-primary" onClick={() => this.listenToEvent()}>Listen</button><br /><br />*/}
                                                            {/*<button className="btn btn-primary" onClick={() => this.stopToEvent()}>Stop</button>*/}
                                                </div>
                                            )
                                        }
                                            {/*<button className="btn btn-primary" onClick={() => this.deleteLivePipeline(data.name)}>{data.na}Delete</button><br /><br />*/}
                                            {/*<button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, 'inactive')}>Activate</button>*/}
                                            {/*<button className="btn btn-primary" onClick={() => this.changeStateLivePipeline(data.name, 'active')}>Deactivate</button><br /><br />*/}
                                            {/*<button className="btn btn-primary" onClick={() => this.getVideoPlayback(data.properties.parameters.find(x => x.name === "videoNameParameter").value, data.name)}>Play video</button>*/}
                                            {/*<button className="btn btn-primary" onClick={() => this.listenToEvent()}>Listen</button><br /><br />*/}
                                            {/*<button className="btn btn-primary" onClick={() => this.stopToEvent()}>Stop</button><br /><br />*/}
                                    </td>
                                    </tr>
                                    <tr>
                                        <td colSpan="6">
                                            <div>
                                                <ul>
                                                    {this.state.events.map((p, i) =>
                                                        <li key={i}>{p}</li>
                                                    )}
                                                </ul>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colSpan="6">
                                            <div>
                                                <div id={"videoRootContainer" + data.name}>
                                                    {/*lva-rtsp-player instances will be added here*/}
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr><td colSpan="6"></td></tr>
                                </div>
                            )}
                    </tbody>
                </table>
                <h5>Add new</h5>
                <form name="livepipeline" onSubmit={(e) => this.createLivePipeline(e)}>
                    <fieldset>
                        <label>Name:</label>&nbsp;
                        <input name="livePipelineName" value={this.state.livePipelineName} onChange={(e) => this.setFormData(e)} />
                    </fieldset>
                    <fieldset >
                        <label>Topology Name:</label>&nbsp;
                         <select name="livePipelineTopologyName" value={this.state.livePipelineTopologyName} onChange={(e) => this.setFormData(e)}>
                            <option value="">Select:</option>
                            {
                                this.state.pipelineTopologies.map((item, index) =>
                                    <option key={index} value={item.name}>{item.name}</option>
                                )
                            }
                        </select>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Url:</label>&nbsp;
                        <input name="rtspUrl" value={this.state.rtspUrl} onChange={(e) => this.setFormData(e)} placeholder="rtsp://rtspsim:554/media/lots_015.mkv"/>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Username:</label>&nbsp;
                        <input name="rtspUsername" value={this.state.rtspUsername} onChange={(e) => this.setFormData(e)} placeholder="username"/>
                    </fieldset>
                    <fieldset >
                        <label>rtsp Password:</label>&nbsp;
                        <input type="password" name="rtspPassword" value={this.state.rtspPassword} onChange={(e) => this.setFormData(e)} placeholder="*******"/>
                    </fieldset>
                    <fieldset >
                        <label>Video Name:</label>&nbsp;
                        <input name="videoName" value={this.state.videoName} onChange={(e) => this.setFormData(e)} placeholder="SampleVideo" />
                    </fieldset>
                    <button type="submit" disabled={!this.state.livePipelineEnabled}>Create</button>
                </form>
            </div>
        );
    }

    renderVideoPlayer(wsHost, websocketToken, pipelineName) {
        let videoId = 0;

        // Dynamically create and add instances of lva-rtsp-player based on input fields. A dummy value for rtspUri is required.
        const createVideo = (id, webSocketUri, authorizationToken) => {
            let player = document.createElement('lva-rtsp-player')
            player.id = "video" + id.toString();
            player.webSocketUri = webSocketUri;
            player.rtspUri = "rtsp://localhost:8554/test";
            player.style.width = "720px";
            player.style.height = "405px";
            player.authorizationToken = authorizationToken;
            let videoRootContainer = document.getElementById("videoRootContainer" + pipelineName);
            videoRootContainer.append(player);
        }

        createVideo(videoId++, wsHost, websocketToken);
    }

    deleteVideoPlayer(pipelineName) {
        let videoRootContainer = document.getElementById("videoRootContainer" + pipelineName);
        while (videoRootContainer.firstChild) {
            videoRootContainer.firstChild.remove();
        }
    }

    render() {
        let pipelineTopologies = this.state.loadingPipelineTopologies
            ? <p><em>Loading PipelineTopologies... </em></p>
            : this.renderPipelineTopologies();

        let livePipelines = this.state.loadingPipelineTopologies && this.state.loadingLivePipelines
            ? <p><em>Loading LivePipelines...</em></p>
            : this.renderLivePipelines();

        return (
            <div>
                <h1 id="tabelLabel" >Edge Devices</h1>
                <p>This component demonstrates fetching Video Analyzers.</p>
                {pipelineTopologies}
                <hr />
                <br />
                {livePipelines}
                <hr />
                <br />
            </div>
        );
    }
}
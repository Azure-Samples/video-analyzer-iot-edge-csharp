import React, {useEffect, Fragment} from 'react';
import PropTypes from 'prop-types';

const ZoneDrawer = props => {
    useEffect(() => {
        if(props.videoName !== '')
        {
            document.body.style.overflowY = "scroll";
            // Get widget container
            const widgetContainer = document.querySelector("#zonedrawer-container");
            widgetContainer.innerHTML = '';
            const output = document.querySelector('#zone-output');
            output.innerText = '';
            // Create new zone drawer widget
            const zoneDrawer = new window.ava.widgets.zoneDrawer();
            widgetContainer.appendChild(zoneDrawer);
            // Create new player widget
            const playerWidget = new window.ava.widgets.player({
                token: props.token,
                clientApiEndpointUrl: props.clientApi,
                videoName: props.videoName,
            });
            // Append the player widget to the zone drawer
            zoneDrawer.appendChild(playerWidget);
            // Load the player widget
            playerWidget.load();
            // Configure the zone drawer
            zoneDrawer.configure({
                locale: "en",
            });
            // Add 'save' event listener when user click save button
            zoneDrawer.addEventListener("ZONE_DRAWER_SAVE", (event) => {
                let currentOutput = '';
                for (const iterator of event?.detail) {
                    currentOutput += JSON.stringify(iterator, null, 2);
                }
                const output = document.querySelector('#zone-output');
                output.value = currentOutput;
            });
            // Load the zone drawer widget
            zoneDrawer.load();
        }
        
    }, [props.videoName]);

    return (
        <div className="containerBlock">
            {
                (props.showOpen) ?
                <Fragment>
                    <div>
                        <label>Zone Information:</label><br/>
                        <textarea className="zone-output" id="zone-output"></textarea>
                    </div>
                    <div id="zonedrawer-container" className="zonedrawer-container"></div>
                </Fragment>
                :
                null
            }
        </div>
    );
};

ZoneDrawer.propTypes = {
    token: PropTypes.string.isRequired,
    clientApi: PropTypes.string.isRequired,
    videoName: PropTypes.string.isRequired,
    showOpen: PropTypes.bool.isRequired
};

export default ZoneDrawer;
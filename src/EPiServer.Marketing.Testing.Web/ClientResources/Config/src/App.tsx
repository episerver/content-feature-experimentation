import React, { useEffect } from "react";
import { ContentArea, Workspace, Typography} from "@episerver/ui-framework";
import "@episerver/ui-framework/dist/main.css";
import "./App.scss";
import ABTestingSettings from "./ABTestingSettings";

function App() {
    return (
        <div className="content-area-container">
            <ContentArea>            
                <Workspace>                
                    <ABTestingSettings/>
                </Workspace>
            </ContentArea>
        </div>
    );
}

export default App;

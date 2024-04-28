import React, { useState, useEffect } from 'react';
import axios from 'axios';

function App() {
  const [responseData, setResponseData] = useState(null);

  useEffect(() => {
    axios.post('http://localhost:5298/DocSum/GetConverstionAll', {
      headers: {
        'Accept': 'text/plain'
      }
    })
      .then(response => {
        setResponseData(response.data);
      })
      .catch(error => {
        console.error('Error fetching data:', error);
      });
  }, []);

  return (
    <div>
      {responseData && (
        <pre>{JSON.stringify(responseData, null, 2)}</pre>
      )}
    </div>
  );
}

export default App;
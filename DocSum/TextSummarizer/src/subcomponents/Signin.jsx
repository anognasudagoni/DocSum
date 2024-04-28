// SignIn.js
import React from 'react';
import { useAuth0 } from '@auth0/auth0-react';

const SignIn = () => {
  const { loginWithRedirect } = useAuth0();

  return (
    <div>
      <button onClick={() => loginWithRedirect()}>Sign In</button>
    </div>
  );
};

export default SignIn;

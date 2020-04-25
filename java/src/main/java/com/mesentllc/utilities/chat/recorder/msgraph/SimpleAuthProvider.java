package com.mesentllc.utilities.chat.recorder.msgraph;

import com.microsoft.graph.authentication.IAuthenticationProvider;
import com.microsoft.graph.http.IHttpRequest;

public class SimpleAuthProvider implements IAuthenticationProvider {
	private final String accessToken;

	SimpleAuthProvider(String accessToken) {
		this.accessToken = accessToken;
	}

	@Override
	public void authenticateRequest(IHttpRequest iHttpRequest) {
		iHttpRequest.addHeader("Authorization", "Bearer " + accessToken);
	}
}

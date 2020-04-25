package com.mesentllc.utilities.chat.recorder.msgraph;

import com.microsoft.aad.msal4j.DeviceCode;
import com.microsoft.aad.msal4j.DeviceCodeFlowParameters;
import com.microsoft.aad.msal4j.IAuthenticationResult;
import com.microsoft.aad.msal4j.PublicClientApplication;
import org.apache.commons.lang3.builder.ReflectionToStringBuilder;
import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

import java.net.MalformedURLException;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;
import java.util.function.Consumer;

public class Authentication {
	private static final Log log = LogFactory.getLog(Authentication.class);
	private static String applicationId;
	private static String authority;

	public static void initialize(String applicationId) {
		Authentication.applicationId = applicationId;
		log.debug("AppId Token set: " + applicationId);
		Authentication.authority = "https://login.microsoftonline.com/organizations/v2.0";
	}

	public static String getAccessToken(String[] scopes) {
		if (applicationId == null) {
			log.error("You must initialize Authentication before calling getAccessToken");
			return null;
		}
		Set<String> scopeSet = new HashSet<>(Arrays.asList(scopes));
		PublicClientApplication app;
		try {
			log.debug("Attempting to build a PublicClientApplication object.");
			app = PublicClientApplication.builder(applicationId).authority(authority).build();
			log.debug("PublicClientApplication: " + new ReflectionToStringBuilder(app).toString());
		}
		catch (MalformedURLException e) {
			log.error("Exception Caught: ", e);
			return null;
		}
		Consumer<DeviceCode> deviceCodeConsumer = (DeviceCode deviceCode) -> log.info(deviceCode.message());
		IAuthenticationResult result = app.acquireToken(DeviceCodeFlowParameters.builder(scopeSet, deviceCodeConsumer).build()).exceptionally(ex -> {
				log.error("Unable to authenticate - " + ex.getMessage());
				return null;
			}).join();
		if (result != null) {
			log.debug("Authentication Token: " + result.accessToken());
			return result.accessToken();
		}
		return null;
	}
}

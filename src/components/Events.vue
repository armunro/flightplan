<template>
  <div class="container mt-4">
    <button v-if="!account" class="btn btn-primary mb-3" @click="signIn">Sign In</button>
    <button v-else class="btn btn-secondary mb-3" @click="signOut">Sign Out</button>

    <ul class="list-group" v-if="events.length">
      <li class="list-group-item" v-for="event in events" :key="event.id">
        <strong>{{ event.subject }}</strong><br />
        {{ new Date(event.start.dateTime).toLocaleString() }} - {{ new Date(event.end.dateTime).toLocaleString() }}
      </li>
    </ul>
    <p v-else-if="account">No events found.</p>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import * as msal from '@azure/msal-browser'
import axios from 'axios'

const clientId = '3c85e963-3398-4c16-b007-d96de320e224' // Replace with your actual client ID

const msalInstance = new msal.PublicClientApplication({
  auth: {
    clientId,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
  },
})

const account = ref(null)
const events = ref([])

const signIn = async () => {
  try {
    const loginResponse = await msalInstance.loginPopup({
      scopes: ['User.Read', 'Calendars.Read'],
    })
    account.value = loginResponse.account
    await getCalendarEvents()
  } catch (err) {
    console.error('Login failed', err)
  }
}

const signOut = () => {
  msalInstance.logoutPopup()
  account.value = null
  events.value = []
}

const getAccessToken = async () => {
  const response = await msalInstance.acquireTokenSilent({
    scopes: ['Calendars.Read'],
    account: msalInstance.getAllAccounts()[0],
  })
  return response.accessToken
}

const getCalendarEvents = async () => {
  try {
    const token = await getAccessToken()
    const res = await axios.get('https://graph.microsoft.com/v1.0/me/events', {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
    events.value = res.data.value
  } catch (err) {
    console.error('Fetching calendar events failed', err)
  }
}

onMounted(async () => {
  await msalInstance.initialize()
  const currentAccounts = msalInstance.getAllAccounts()
  if (currentAccounts.length > 0) {
    account.value = currentAccounts[0]
    await getCalendarEvents()
  }
})
</script>

<style scoped>
h3 {
  margin-bottom: 1rem;
}
</style>

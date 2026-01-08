'use client'

import { useEffect, useState } from 'react'

type WeatherForecast = {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string
}

export default function Home() {
  const [data, setData] = useState<WeatherForecast[]>([])

  useEffect(() => {
    fetch('http://localhost:5290/weatherforecast')
      .then(res => res.json())
      .then(data => setData(data))
      .catch(err => console.error(err))
  }, [])

  return (
    <main style={{ padding: 20 }}>
      <h1>Test FE gọi BE</h1>
      <pre>{JSON.stringify(data, null, 2)}</pre>
    </main>
  )
}
